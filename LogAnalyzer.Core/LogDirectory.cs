﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using JetBrains.Annotations;
using LogAnalyzer.Config;
using LogAnalyzer.Extensions;
using System.Threading;
using LogAnalyzer.Collections;
using LogAnalyzer.Filters;
using LogAnalyzer.Kernel;

namespace LogAnalyzer
{
	public sealed class LogDirectory : LogEntriesList, ILogVisitable
	{
		private readonly object sync = new object();
		private readonly IList<LogFile> files = CollectionHelper.CreateList<LogFile>();

		private readonly ObservableList<LogFile> filesWrapper;
		public ObservableList<LogFile> Files
		{
			get { return filesWrapper; }
		}

		internal override int TotalFilesCount
		{
			get { return files.Count; }
		}

		private readonly LogNotificationsSourceBase _operationsSource;
		private readonly IEnvironment _environment;
		private readonly IDirectoryInfo _directoryInfo;
		private readonly IOperationsQueue _operationsQueue;

		private readonly LogAnalyzerConfiguration _config;
		private readonly LogAnalyzerCore _core;
		private readonly ExpressionFilter<IFileInfo> _fileFilter;
		private readonly IFilter<LogEntry> _globalEntriesFilter;
		private readonly Encoding encoding = Encoding.Unicode;
		private readonly ILogLineParser _lineParser;

		public ILogLineParser LineParser
		{
			get { return _lineParser; }
		}

		public IFilter<LogEntry> GlobalEntriesFilter
		{
			get { return _globalEntriesFilter; }
		}

		public ExpressionFilter<IFileInfo> FileFilter
		{
			get { return _fileFilter; }
		}

		public LogAnalyzerConfiguration Config
		{
			get { return _config; }
		}

		public Encoding Encoding
		{
			get { return encoding; }
		}

		private readonly LogDirectoryConfigurationInfo directoryConfig;
		public LogDirectoryConfigurationInfo DirectoryConfig
		{
			get { return directoryConfig; }
		}

		/// <summary>
		/// Path to directory.
		/// </summary>
		public string Path { get; private set; }
		public string FileNameFilter { get; private set; }
		public string DisplayName { get; private set; }
		public bool UseCache { get; private set; }

		public LogDirectory( [NotNull]LogDirectoryConfigurationInfo directoryConfigurationInfo, [NotNull]LogAnalyzerConfiguration config,
			[NotNull]IEnvironment environment, [NotNull]LogAnalyzerCore core )
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

			this.directoryConfig = directoryConfigurationInfo;
			this.Path = directoryConfigurationInfo.Path;
			this.FileNameFilter = directoryConfigurationInfo.FileNameFilter;
			this.DisplayName = directoryConfigurationInfo.DisplayName;
			this.UseCache = directoryConfigurationInfo.UseCache;

			this._directoryInfo = environment.GetDirectory( Path );
			this._operationsSource = _directoryInfo.NotificationSource;
			this._environment = environment;
			this._operationsQueue = environment.OperationsQueue;
			this._config = config;
			this._core = core;
			this.filesWrapper = new ObservableList<LogFile>( files );
			this._globalEntriesFilter = config.GlobalLogEntryFilter;
			this._lineParser = directoryConfigurationInfo.LineParser ?? new ManualLogLineParser();
			this._fileFilter = config.GlobalFilesFilter;

			_fileFilter.Changed += OnFileFilterChanged;

			_operationsSource.Changed += OnFileChanged;
			_operationsSource.Created += OnFileCreated;
			_operationsSource.Deleted += OnFileDeleted;
			_operationsSource.Error += OnWatcherError;
			_operationsSource.Renamed += OnFileRenamed;

			if ( !String.IsNullOrWhiteSpace( directoryConfigurationInfo.EncodingName ) )
			{
				this.encoding = Encoding.GetEncoding( directoryConfigurationInfo.EncodingName );
			}
		}

		private void OnFileFilterChanged( object sender, EventArgs e )
		{
			throw new NotImplementedException();
		}

		protected override void StartImpl()
		{
			IDirectoryInfo dir = _environment.GetDirectory( Path );

			// отсекаем ситуации, когда по фильтру *.log возвращаются файлы *.log__
			int extensionLength = 100;
			if ( !String.IsNullOrEmpty( FileNameFilter ) && FileNameFilter != "*" && FileNameFilter.Contains( '.' ) )
			{
				// для трехбуквенного extension будет 4
				extensionLength = FileNameFilter.Length - FileNameFilter.LastIndexOf( '.' );
			}

			var filesInDirectory = (from file in dir.EnumerateFiles( FileNameFilter )
									where file.Extension.Length <= extensionLength // Например, file.Extension = ".log"
									select file).ToList();

			BeginLoadFiles( filesInDirectory );
		}

		private int initialFilesLoadedCount;
		private bool loadStarted;
		private int initialFilesLoadingCount;
		private void BeginLoadFiles( IEnumerable<IFileInfo> filesInDirectory )
		{
			foreach ( IFileInfo file in filesInDirectory )
			{
				file.Refresh();

				if ( !_fileFilter.Include( file ) )
					continue;

				IFileInfo local = file;

				LogFile logFile = CreateLogFile( local );

				_operationsQueue.EnqueueOperation( () => AddFile( logFile ) );

				_environment.Scheduler.StartNewOperation( () =>
				{
					logFile.ReadFile();

					_operationsQueue.EnqueueOperation( () =>
					{
						Logger.WriteInfo( "Loaded file \"{0}\"", local.Name );
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
				_environment.Scheduler.StartNewOperation( () =>
				{
					PerformInitialMerge();

					_operationsSource.Start();
					_operationsQueue.EnqueueOperation( () =>
					{
						Logger.WriteInfo( "LogDirectory \"{0}\": loaded {1} file(s).",
											this.DisplayName, files.Count );

						RaiseLoadedEvent();
					} );
				} );
			}
		}

		private void PerformInitialMerge()
		{
			PerformInitialMerge( files.Sum( f => f.LogEntries.Count ),
				files.SelectMany( f => f.LogEntries ) );
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

			_operationsQueue.EnqueueOperation( () => OnLogEntryAddedToFileHandler( addedEntries ) );
		}

		/// <summary>
		/// Происходит в потоке FileSystemWatcherEventsThread.
		/// </summary>
		/// <param name="addedEntries"></param>
		private void OnLogEntryAddedToFileHandler( IList<LogEntry> addedEntries )
		{
			EnqueueToMerge( addedEntries );
			_core.EnqueueToMerge( addedEntries );
		}

		#region FileSystemWatcher notifications

		private LogFile CreateLogFile( IFileInfo file )
		{
			LogFile logFile = new LogFile( file, this );

			logFile.ReadProgress += OnLogFileReadProgress;

			return logFile;
		}

		private void OnLogFileReadProgress( object sender, FileReadEventArgs e )
		{
			RaiseReadProgress( e );
		}

		private void OnFileCreated( object sender, FileSystemEventArgs e )
		{
			Logger.WriteInfo( "Core.OnFileCreated: '{0}' '{1}'", e.ChangeType, e.Name );

			string fullPath = e.FullPath;
			_operationsQueue.EnqueueOperation( () =>
			{
				if ( !ContainsFile( fullPath ) )
				{
					// добавляем в список отслеживаемых файлов
					// todo обработка исключений
					AddFile( fullPath );
				}
			} );
		}

		private void AddFile( string fullPath )
		{
			lock ( sync )
			{
				IFileInfo file = _directoryInfo.GetFileInfo( fullPath );

				bool exclude = !_fileFilter.Include( file );
				if ( exclude )
				{
					Logger.WriteInfo( "AddFile: Excluded file '{0}'", fullPath );
					return;
				}

				LogFile logFile = CreateLogFile( file );

				// todo brinchuk this should be done async!
				logFile.ReadFile();

				Condition.DebugAssert( !files.Contains( logFile ) );
				files.Add( logFile );
				filesWrapper.RaiseCollectionAdded( logFile );

				logFile.LogEntries.RaiseCollectionReset();
			}
		}

		private void OnFileDeleted( object sender, FileSystemEventArgs e )
		{
			Logger.WriteInfo( "Core.OnFileDelete: '{0}' '{1}'", e.ChangeType, e.Name );
		}

		private void OnFileChanged( object sender, FileSystemEventArgs e )
		{
			Logger.DebugWriteInfo( "Core.OnFileChanged: '{0}' '{1}'", e.ChangeType, e.Name );

			string fullPath = e.FullPath;

			_operationsQueue.EnqueueOperation( () =>
			{
				if ( !ContainsFile( fullPath ) )
				{
					AddFile( fullPath );
				}
				else
				{
					var changedFile = files.Single( f => f.FullPath == fullPath );

					if ( !_fileFilter.Include( changedFile.FileInfo ) )
						return;

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

		private void AddFile( LogFile logFile )
		{
			if ( logFile == null )
				throw new ArgumentNullException( "logFile" );

			Condition.DebugAssert( !files.Contains( logFile ) );

			files.Add( logFile );

			OnLogEntriesAddedToFile( logFile.LogEntries );
		}

		public override long TotalLengthInBytes
		{
			get
			{
				long result = filesWrapper.Sum( f => f.TotalLengthInBytes );
				return result;
			}
		}

		public void Accept( ILogVisitor visitor )
		{
			visitor.Visit( this );
		}
	}
}
