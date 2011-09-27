﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.IO;
using LogAnalyzer.Config;
using LogAnalyzer.Extensions;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using LogAnalyzer.Collections;
using LogAnalyzer.Filters;
using LogAnalyzer.Caching;
using LogAnalyzer.Kernel;
using System.Reactive.Threading.Tasks;

namespace LogAnalyzer
{
	public sealed class LogDirectory : LogEntriesList
	{
		private readonly object sync = new object();
		private readonly IList<LogFile> files = CollectionHelper.CreateList<LogFile>();

		private readonly ThinListWrapper<LogFile> filesWrapper;
		public ThinListWrapper<LogFile> Files
		{
			get { return filesWrapper; }
		}

		internal override int TotalFilesCount
		{
			get { return files.Count; }
		}

		private readonly LogNotificationsSourceBase operationsSource;
		private readonly IEnvironment environment;
		private readonly IDirectoryInfo directoryInfo;
		private readonly IOperationsQueue operationsQueue;

		private readonly LogAnalyzerConfiguration config;
		private readonly LogAnalyzerCore core;
		private readonly ExpressionFilter<IFileInfo> fileFilter = new ExpressionFilter<IFileInfo>();
		private readonly IFilter<LogEntry> globalEntriesFilter;
		private readonly Encoding encoding = Encoding.Unicode;

		public IFilter<LogEntry> GlobalEntriesFilter
		{
			get { return globalEntriesFilter; }
		}

		// todo внести в настройки
		public ExpressionFilter<IFileInfo> FileFilter
		{
			get { return fileFilter; }
		}

		public LogAnalyzerConfiguration Config
		{
			get { return config; }
		}

		public Encoding Encoding
		{
			get { return encoding; }
		}

		/// <summary>
		/// Path to directory.
		/// </summary>
		public string Path { get; private set; }
		public string FileNameFilter { get; private set; }
		public string DisplayName { get; private set; }
		public bool UseCache { get; private set; }

		internal LogDirectory( LogDirectoryConfigurationInfo directoryConfigurationInfo, LogAnalyzerConfiguration config, IEnvironment environment, LogAnalyzerCore core )
			: base( environment, config.Logger )
		{
			if ( directoryConfigurationInfo == null )
				throw new ArgumentNullException( "directoryConfigurationInfo" );
			if ( config == null )
				throw new ArgumentNullException( "config" );
			if ( environment == null )
				throw new ArgumentNullException( "environment" );
			if ( core == null )
				throw new ArgumentNullException( "core" );

			this.Path = directoryConfigurationInfo.Path;
			this.FileNameFilter = directoryConfigurationInfo.FileNameFilter;
			this.DisplayName = directoryConfigurationInfo.DisplayName;
			this.UseCache = directoryConfigurationInfo.UseCache;

			this.directoryInfo = environment.GetDirectory( Path );
			this.operationsSource = directoryInfo.NotificationSource;
			this.environment = environment;
			this.operationsQueue = environment.OperationsQueue;
			this.config = config;
			this.core = core;
			this.filesWrapper = new ThinListWrapper<LogFile>( files );
			this.globalEntriesFilter = config.GlobalLogEntryFilter;

			fileFilter.Changed += OnFileFilterChanged;

			operationsSource.Changed += OnFileChanged;
			operationsSource.Created += OnFileCreated;
			operationsSource.Deleted += OnFileDeleted;
			operationsSource.Error += OnWatcherError;
			operationsSource.Renamed += OnFileRenamed;

			if ( !String.IsNullOrWhiteSpace( directoryConfigurationInfo.EncodingName ) )
			{
				this.encoding = Encoding.GetEncoding( directoryConfigurationInfo.EncodingName );
			}
		}

		private void OnFileFilterChanged( object sender, EventArgs e )
		{
			throw new NotImplementedException();
		}

		protected override void StartCore()
		{
			IDirectoryInfo dir = environment.GetDirectory( Path );

			// отсекаем ситуации, когда по фильтру *.log возвращаются файлы *.log__
			int extensionLength = 100;
			if ( FileNameFilter != "*" && FileNameFilter.Contains( '.' ) )
			{
				// для трехбуквенного extension будет 4
				extensionLength = FileNameFilter.Length - FileNameFilter.LastIndexOf( '.' );
			}

			var filesInDirectory = ( from file in dir.EnumerateFiles( FileNameFilter )
									 where file.Extension.Length <= extensionLength // Например, file.Extension = ".log"
									 select file ).ToList();

			BeginLoadFiles( filesInDirectory );
		}

		private int initialFilesLoadedCount;
		private bool loadStarted = false;
		private int initialFilesLoadingCount;
		private void BeginLoadFiles( IEnumerable<IFileInfo> filesInDirectory )
		{
			foreach ( IFileInfo file in filesInDirectory )
			{
				file.Refresh();

				// do not analyze files that were updated too long ago
				if ( !fileFilter.Include( file ) )
					continue;

				IFileInfo local = file;

				LogFile logFile = CreateLogFile( local );

				operationsQueue.EnqueueOperation( () =>
				{
					AddFile( logFile );
				} );

				environment.Scheduler.StartNewOperation( () =>
					{
						logFile.ReadFile();

						operationsQueue.EnqueueOperation( () =>
						{
							logger.WriteInfo( "Loaded file \"{0}\"", local.Name );
							Interlocked.Increment( ref initialFilesLoadedCount );
							AnalyzeIfLoaded();
						} );
					} );

				//// todo какая-то обработка ошибки чтения файла
				//fileReadTask.ContinueWith( parentTask =>
				//{
				//    parentTask.Exception.Handle( e => e is LogAnalyzerIOException );

				//    throw new NotImplementedException();
				//}, TaskContinuationOptions.OnlyOnFaulted );

				Interlocked.Increment( ref initialFilesLoadingCount );
			}

			loadStarted = true;
			AnalyzeIfLoaded();
		}

		private void AnalyzeIfLoaded()
		{
			if ( !loadStarted )
				return;

			// все файлы в начальной загрузке загружены?
			if ( initialFilesLoadedCount == initialFilesLoadingCount )
			{
				environment.Scheduler.StartNewOperation( () =>
														{
															PerformInitialMerge();

															operationsSource.Start();
															operationsQueue.EnqueueOperation( () =>
																								{
																									logger.WriteInfo( "LogDirectory \"{0}\": loaded {1} file(s).",
																													 this.DisplayName, files.Count );

																									RaiseLoadedEvent();
																								} );
														} );
			}
		}

		private void PerformInitialMerge()
		{
			// todo не сделать ли тут запас по Capacity?
			MergedEntriesList.Capacity = files.Sum( f => f.LogEntries.Count );

			LogEntry[] sortedEntries = files
				.SelectMany( f => f.LogEntries )
				.AsParallel()
				.OrderBy( LogEntryByDateComparer.Instance )
				.ToArray();

#if ASSERT
			Condition.DebugAssert( sortedEntries.AreSorted( LogEntryByDateComparer.Instance ) );
#endif

			// ILSpy: используется Buffer.BlockCopy
			MergedEntriesList.AddRange( sortedEntries );
			MergedEntries.RaiseCollectionReset();
		}

		internal void OnLogEntriesAddedToFile( IList<LogEntry> addedEntries )
		{
			if ( !IsLoaded )
			{
				// ничего не делаем, все равно потом будет общий мердж
				return;
			}

			if ( addedEntries.Count == 0 )
				return;

			operationsQueue.EnqueueOperation( () =>
			{
				OnLogEntryAddedToFileHandler( addedEntries );
			} );
		}

		/// <summary>
		/// Происходит в потоке FileSystemWatcherEventsThread.
		/// </summary>
		/// <param name="addedEntries"></param>
		private void OnLogEntryAddedToFileHandler( IList<LogEntry> addedEntries )
		{
			EnqueueToMerge( addedEntries );
			core.EnqueueToMerge( addedEntries );
		}

		#region FileSystemWatcher notifications

		private LogFile CreateLogFile( IFileInfo file )
		{
			LogFile logFile = new LogFile( file, this );

			logFile.ReadProgress += OnLogFile_FileReadProgress;

			return logFile;
		}

		private void OnLogFile_FileReadProgress( object sender, FileReadEventArgs e )
		{
			RaiseReadProgress( e );
		}

		private void OnFileCreated( object sender, FileSystemEventArgs e )
		{
			logger.WriteInfo( "Core.OnFileCreated: '{0}' '{1}'", e.ChangeType, e.Name );

			string fullPath = e.FullPath;
			operationsQueue.EnqueueOperation( () =>
			{
				if ( !ContainsFile( fullPath ) )
				{
					// добавляем в список отслеживаемых файлов
					// todo обработка исключений
					AddFile( fullPath );
				}
				else
				{
					// error!
					logger.WriteError( "Watcher notified that the file \"{0}\" was created, but it is already present in 'files' dictionary.", fullPath );
				}
			} );
		}

		private void AddFile( string fullPath )
		{
			LogFile logFile;
			lock ( sync )
			{
				IFileInfo file = directoryInfo.GetFileInfo( fullPath );
				logFile = CreateLogFile( file );

				// todo brinchuk this should be done async!
				logFile.ReadFile();

				AddFile( logFile );
			}

			//Task.Factory.StartNew( logFile.ReadFile );
		}

		private void OnFileDeleted( object sender, FileSystemEventArgs e )
		{
			logger.WriteInfo( "Core.OnFileDelete: '{0}' '{1}'", e.ChangeType, e.Name );
			// todo implement Core.OnFileDeleted!
			throw new NotImplementedException();
		}

		private void OnFileChanged( object sender, FileSystemEventArgs e )
		{
			logger.DebugWriteInfo( "Core.OnFileChanged: '{0}' '{1}'", e.ChangeType, e.Name );

			string fullPath = e.FullPath;

			operationsQueue.EnqueueOperation( () =>
			{
				if ( !ContainsFile( fullPath ) )
				{
					logger.WriteError( "Core.OnFileChanged: files doesn't contain file \"{0}\"", fullPath );
					AddFile( fullPath );
					return;
				}
				else
				{
					var changedFile = files.Single( f => f.FullPath == fullPath );
					changedFile.OnFileChanged();
				}
			} );
		}

		private void OnFileRenamed( object sender, RenamedEventArgs e )
		{
			// todo
			throw new NotImplementedException();
		}

		private void OnWatcherError( object sender, ErrorEventArgs e )
		{
			// todo
			throw new NotImplementedException();
		}

		#endregion


		public bool ContainsFile( string fullPath )
		{
			return files.Any( f => f.FullPath == fullPath );
		}

		public void AddFile( LogFile logFile )
		{
			if ( logFile == null )
				throw new ArgumentNullException( "logFile" );

			Condition.DebugAssert( !files.Contains( logFile ) );

			files.Add( logFile );

			OnLogEntriesAddedToFile( logFile.LogEntries );
		}

		public override int TotalLengthInBytes
		{
			get
			{
				int result = files.Sum( f => f.TotalLengthInBytes );
				return result;
			}
		}
	}
}
