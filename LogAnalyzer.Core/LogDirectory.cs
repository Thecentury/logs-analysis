using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using JetBrains.Annotations;
using LogAnalyzer.Common;
using LogAnalyzer.Config;
using LogAnalyzer.Extensions;
using System.Threading;
using LogAnalyzer.Collections;
using LogAnalyzer.Filters;
using LogAnalyzer.Kernel;
using LogAnalyzer.Kernel.Notifications;
using LogAnalyzer.Kernel.Parsers;
using LogAnalyzer.Logging;
using IOPath = System.IO.Path;
using LogAnalyzer.Operations;

namespace LogAnalyzer
{
	public sealed class LogDirectory : LogEntriesList, ILogVisitable
	{
		private readonly object _sync = new object();
		private readonly IList<LogFile> _files = CollectionHelper.CreateList<LogFile>();

		private readonly ObservableList<LogFile> _filesWrapper;
		public ObservableList<LogFile> Files
		{
			get { return _filesWrapper; }
		}

		internal override int TotalFilesCount
		{
			get { return _files.Count; }
		}

		private readonly LogNotificationsSourceBase _notificationsSource;
		private readonly IEnvironment _environment;
		private readonly IDirectoryInfo _directoryInfo;
		private readonly IOperationsQueue _operationsQueue;

		private readonly LogAnalyzerConfiguration _config;
		private readonly LogAnalyzerCore _core;

		private readonly ExpressionFilter<IFileInfo> _globalFileFilter;
		private readonly ExpressionFilter<string> _globalFileNameFilter;

		private readonly Encoding _encoding = Encoding.Unicode;
		private readonly ILogLineParser _lineParser;

		private readonly ImpersonationContext _impersonationContext;

		public ILogLineParser LineParser
		{
			get { return _lineParser; }
		}

		public LogNotificationsSourceBase NotificationsSource
		{
			get { return _notificationsSource; }
		}

		public ExpressionFilter<LogEntry> EntriesFilter { get; set; }

		public ExpressionFilter<IFileInfo> LocalFileFilter { get; set; }

		public ExpressionFilter<string> LocalFileNameFilter { get; set; }

		public IFilter<IFileInfo> FileFilter
		{
			get { return _globalFileFilter; }
		}

		public LogAnalyzerConfiguration Config
		{
			get { return _config; }
		}

		public Encoding Encoding
		{
			get { return _encoding; }
		}

		private readonly LogDirectoryConfigurationInfo _directoryConfig;
		public LogDirectoryConfigurationInfo DirectoryConfig
		{
			get { return _directoryConfig; }
		}

		/// <summary>
		/// Path to directory.
		/// </summary>
		public string Path { get; private set; }
		public string DisplayName { get; private set; }
		public bool UseCache { get; private set; }

		private LogDirectory()
		{
			_filesWrapper = new ObservableList<LogFile>();
		}

		internal static LogDirectory CreateEmpty( string displayName )
		{
			var logDirectory = new LogDirectory { DisplayName = displayName };
			return logDirectory;
		}

		public LogDirectory( [NotNull]LogDirectoryConfigurationInfo directoryCfg, [NotNull]LogAnalyzerConfiguration config,
			[NotNull]IEnvironment environment, [NotNull]LogAnalyzerCore core )
			: base( environment, config.Logger )
		{
			if ( directoryCfg == null )
			{
				throw new ArgumentNullException( "directoryCfg" );
			}
			if ( config == null )
			{
				throw new ArgumentNullException( "config" );
			}
			if ( environment == null )
			{
				throw new ArgumentNullException( "environment" );
			}
			if ( core == null )
			{
				throw new ArgumentNullException( "core" );
			}

			this._directoryConfig = directoryCfg;
			this.Path = directoryCfg.Path;
			this.DisplayName = directoryCfg.DisplayName;
			this.UseCache = directoryCfg.UseCache;

			this._directoryInfo = environment.GetDirectory( Path );
			this._notificationsSource = _directoryInfo.NotificationSource;
			this._environment = environment;
			this._operationsQueue = environment.OperationsQueue;
			this._config = config;
			this._core = core;
			this._filesWrapper = new ObservableList<LogFile>( _files );
			this._lineParser = directoryCfg.LineParser ?? new ManualLogLineParser();

			this.EntriesFilter = new ExpressionFilter<LogEntry>(
				new And(
					config.GlobalLogEntryFilter.ExpressionBuilder,
					directoryCfg.LogEntriesFilter ?? new AlwaysTrue() ) );

			this._globalFileFilter = config.GlobalFilesFilter;
			this._globalFileNameFilter = config.GlobalFileNamesFilter;
			this.LocalFileFilter = new ExpressionFilter<IFileInfo>( directoryCfg.FilesFilter ?? new AlwaysTrue() );
			this.LocalFileNameFilter = new ExpressionFilter<string>( directoryCfg.FileNamesFilter ?? new AlwaysTrue() );

			_globalFileFilter.Changed += OnFileFilterChanged;

			_notificationsSource.Changed += OnFileChanged;
			_notificationsSource.Created += OnFileCreated;
			_notificationsSource.Deleted += OnFileDeleted;
			_notificationsSource.Error += OnWatcherError;
			_notificationsSource.Renamed += OnFileRenamed;

			if ( !String.IsNullOrWhiteSpace( directoryCfg.EncodingName ) )
			{
				this._encoding = Encoding.GetEncoding( directoryCfg.EncodingName );
			}

			if ( !directoryCfg.Domain.IsNullOrEmpty() && !directoryCfg.Username.IsNullOrEmpty() && !directoryCfg.Password.IsNullOrEmpty() )
			{
				_impersonationContext = new ImpersonationContext( directoryCfg.Domain, directoryCfg.Username, directoryCfg.Password );
			}
		}

		private void OnFileFilterChanged( object sender, EventArgs e )
		{
		}

		protected override void StartImpl()
		{
			_operationsQueue.EnqueueOperation( _impersonationContext, () =>
			{
				IDirectoryInfo dir = _environment.GetDirectory( Path );

				var filesInDirectory = (from path in dir.EnumerateFileNames()
										let fileName = IOPath.GetFileNameWithoutExtension( path )
										where _globalFileNameFilter.Include( fileName )
										where LocalFileNameFilter.Include( fileName )
										let file = dir.GetFileInfo( path )
										select file).ToList();

				BeginLoadFiles( filesInDirectory );
			} );
		}

		private int _initialFilesLoadedCount;
		private bool _loadStarted;
		private int _initialFilesLoadingCount;

		private void BeginLoadFiles( IEnumerable<IFileInfo> filesInDirectory )
		{
#if false
			List<IBidirectionalEnumerable<LogEntry>> enumerators = new List<IBidirectionalEnumerable<LogEntry>>();

			foreach ( var file in filesInDirectory )
			{
				file.Refresh();

				if ( !_fileFilter.Include( file ) )
				{
					continue;
				}

				LogFile logFile = CreateLogFile( file );
				enumerators.Add( logFile.GetNavigator() );
			}

			MergingNavigator merger = new MergingNavigator( enumerators );

			RaiseLoadedEvent();
#endif

			using ( _impersonationContext.ExecuteInContext() )
			{
				foreach ( IFileInfo file in filesInDirectory )
				{
					file.Refresh();

					if ( !_globalFileFilter.Include( file ) )
					{
						continue;
					}

					IFileInfo local = file;

					LogFile logFile = CreateLogFile( local );

					_operationsQueue.EnqueueOperation( () => AddFile( logFile ) );

					_environment.Scheduler.StartNewOperation( _impersonationContext, () =>
					{
						logFile.ReadFile();

						_operationsQueue.EnqueueOperation( () =>
						{
							Logger.WriteInfo(
								"Loaded file '{0}'", local.Name );
							Interlocked.Increment(
								ref _initialFilesLoadedCount );
							AnalyzeIfLoaded();
						} );
					} );

					Interlocked.Increment( ref _initialFilesLoadingCount );
				}

				_loadStarted = true;
				AnalyzeIfLoaded();
			}
		}

		private void AnalyzeIfLoaded()
		{
			if ( !_loadStarted )
			{
				return;
			}

			// все файлы в начальной загрузке загружены?
			if ( _initialFilesLoadedCount == _initialFilesLoadingCount )
			{
				_environment.Scheduler.StartNewOperation( () =>
				{
					PerformInitialMerge();

					if ( DirectoryConfig.NotificationsEnabledAtStart )
					{
						_notificationsSource.Start();
					}

					_operationsQueue.EnqueueOperation( () =>
					{
						Logger.WriteInfo( "LogDirectory '{0}': loaded {1} file(s).",
											this.DisplayName, _files.Count );

						RaiseLoadedEvent();
					} );
				} );
			}
		}

		private void PerformInitialMerge()
		{
			PerformInitialMerge( _files.Sum( f => f.LogEntries.Count ),
				_files.SelectMany( f => f.LogEntries ) );
		}

		internal void OnLogEntriesAddedToFile( IList<LogEntry> addedEntries )
		{
			if ( !IsLoaded )
			{
				// ничего не делаем, все равно потом будет общий мердж
				return;
			}

			if ( addedEntries.Count == 0 )
			{
				return;
			}

			_operationsQueue.EnqueueOperation( () => OnLogEntriesAddedToFileInner( addedEntries ) );
		}

		/// <summary>
		/// Происходит в потоке FileSystemWatcherEventsThread.
		/// </summary>
		/// <param name="addedEntries"></param>
		private void OnLogEntriesAddedToFileInner( IList<LogEntry> addedEntries )
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
			_operationsQueue.EnqueueOperation( _impersonationContext, () =>
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
			lock ( _sync )
			{
				try
				{
					IFileInfo file = null;
					if ( !IncludeFile( fullPath, ref file ) )
					{
						return;
					}

					LogFile logFile = CreateLogFile( file );

					// todo brinchuk this should be done async!
					logFile.ReadFile();

					Condition.DebugAssert( !_files.Contains( logFile ) );
					_files.Add( logFile );
					_filesWrapper.RaiseCollectionAdded( logFile );

					logFile.LogEntries.RaiseCollectionReset();
				}
				catch ( Exception exc )
				{
					Logger.WriteError( "LogDirectory.AddFile(): failed with {0}", exc );
				}
			}
		}

		[Pure]
		private bool IncludeFile( string fullPath, ref IFileInfo file )
		{
			string fileName = IOPath.GetFileNameWithoutExtension( fullPath );
			if ( !_globalFileNameFilter.Include( fileName ) )
			{
				Logger.WriteInfo( "AddFile: Excluded file '{0}' by global filter by its name", fullPath );
				return false;
			}

			if ( !LocalFileNameFilter.Include( fileName ) )
			{
				Logger.WriteInfo( "AddFile: Excluded file '{0}' by local filter by its name", fullPath );
				return false;
			}

			if ( file == null )
			{
				file = _directoryInfo.GetFileInfo( fullPath );
			}

			if ( !_globalFileFilter.Include( file ) )
			{
				Logger.WriteInfo( "AddFile: Excluded file '{0}' by global filter", fullPath );
				return false;
			}

			if ( !LocalFileFilter.Include( file ) )
			{
				Logger.WriteInfo( "AddFile: Excluded file '{0}' by local filter", fullPath );
				return false;
			}

			return true;
		}

		private void OnFileDeleted( object sender, FileSystemEventArgs e )
		{
			Logger.WriteInfo( "Core.OnFileDelete: '{0}'", e.Name );
		}

		private void OnFileChanged( object sender, FileSystemEventArgs e )
		{
			Logger.DebugWriteInfo( "Core.OnFileChanged: '{0}' '{1}'", e.ChangeType, e.Name );

			string fullPath = e.FullPath;

			_operationsQueue.EnqueueOperation( _impersonationContext, () =>
			{
				if ( !ContainsFile( fullPath ) )
				{
					AddFile( fullPath );
				}
				else
				{
					var changedFile = _files.Single( f => f.FullPath == fullPath );
					var fileInfo = changedFile.FileInfo;

					if ( !IncludeFile( fullPath, ref fileInfo ) )
					{
						return;
					}

					changedFile.OnFileChanged();
				}
			} );
		}

		private void OnFileRenamed( object sender, RenamedEventArgs e )
		{
			Logger.WriteError( "File was renamed, which is unsupported. {0} -> {1}", e.OldFullPath, e.FullPath );
		}

		private void OnWatcherError( object sender, ErrorEventArgs e )
		{
			Logger.WriteError( "OnWatcherError: {0}", e.GetException() );
		}

		#endregion

		public bool ContainsFile( string fullPath )
		{
			return _files.Any( f => f.FullPath == fullPath );
		}

		private void AddFile( LogFile logFile )
		{
			if ( logFile == null )
			{
				throw new ArgumentNullException( "logFile" );
			}

			Condition.DebugAssert( !_files.Contains( logFile ) );
			Condition.Assert( !_files.Any( f => f.FullPath == logFile.FullPath ) );

			_files.Add( logFile );

			OnLogEntriesAddedToFile( logFile.LogEntries );
		}

		public override long TotalLengthInBytes
		{
			get
			{
				long result = _filesWrapper.Sum( f => f.TotalLengthInBytes );
				return result;
			}
		}

		public void Accept( ILogVisitor visitor )
		{
			visitor.Visit( this );
		}
	}
}
