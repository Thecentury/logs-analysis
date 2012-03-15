using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Collections.ObjectModel;
using JetBrains.Annotations;
using LogAnalyzer.Config;
using LogAnalyzer.Extensions;
using LogAnalyzer.Kernel;

namespace LogAnalyzer
{
	public sealed class LogAnalyzerCore : LogEntriesList, ILogVisitable
	{
		private readonly LogAnalyzerConfiguration _config;
		private readonly IOperationsQueue _operationsQueue;

		public IOperationsQueue OperationsQueue
		{
			get { return _operationsQueue; }
		}

		private readonly List<LogDirectory> _directories = new List<LogDirectory>();
		private readonly ReadOnlyCollection<LogDirectory> _readonlyDirectories;
		public IList<LogDirectory> Directories
		{
			get { return _readonlyDirectories; }
		}

		public void AddDirectory( [NotNull] LogDirectory dir )
		{
			if ( dir == null )
			{
				throw new ArgumentNullException( "dir" );
			}

			dir.Loaded += OnDirectoryLoaded;
			dir.ReadProgress += OnDirectoryReadProgress;
			_directories.Add( dir );
		}

		public void RemoveDirectory( [NotNull] LogDirectory dir )
		{
			if ( dir == null ) throw new ArgumentNullException( "dir" );

			dir.Loaded -= OnDirectoryLoaded;
			dir.ReadProgress -= OnDirectoryReadProgress;
			_directories.Remove( dir );
		}

		internal override int TotalFilesCount
		{
			get { return _directories.Sum( d => d.TotalFilesCount ); }
		}

		public LogAnalyzerConfiguration Config
		{
			get { return _config; }
		}

		// todo заменить NotImplementedException на NotSupportedException
		// todo открывать в VS файл на нужной строке (где исключение)

		//string vsCommandLine = @"c:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\devenv.exe";
		// todo не работает переход на заданную строку
		//string vsArgs = "/edit " + @"""C:\Development\HttpServer\HttpServer\Server.cs""" + " /command \"Edit.Find 123\"";//\"Edit.GoTo 150\"";

		//Process.Start(vsCommandLine, vsArgs);
		//Environment.Exit(0);

		private int _directoriesLoadedCount;

		public LogAnalyzerCore( LogAnalyzerConfiguration config, IEnvironment environment )
			: base( environment, config.Logger )
		{
			if ( config == null )
			{
				throw new ArgumentNullException( "config" );
			}
			if ( config.Logger == null )
			{
				throw new ArgumentException( "config.Logger should not be null." );
			}
			if ( environment == null )
			{
				throw new ArgumentNullException( "environment" );
			}

			this._config = config;
			config.Directories.CollectionChanged += OnConfigDirectoriesCollectionChanged;

			Logger.DebugWriteInfo( "" );
			Logger.DebugWriteInfo( "" );
			Logger.DebugWriteInfo( "" );

			foreach ( LogDirectoryConfigurationInfo dir in config.EnabledDirectories )
			{
				AddDirectory( dir );
			}

			this._readonlyDirectories = _directories.AsReadOnly();

			this._operationsQueue = environment.OperationsQueue;
		}

		private void AddDirectory( LogDirectoryConfigurationInfo dir )
		{
			if ( String.IsNullOrWhiteSpace( dir.EncodingName ) )
			{
				dir.EncodingName = _config.DefaultEncodingName;
			}

			var logDirectory = new LogDirectory( dir, _config, Environment, this );
			AddDirectory( logDirectory );
		}

		private void OnConfigDirectoriesCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			if ( e.Action != NotifyCollectionChangedAction.Add )
			{
				throw new NotSupportedException( "Collection change actions other than Add are not supported." );
			}
			if ( HaveStarted )
			{
				throw new InvalidOperationException( "Collection changes are not allowed after core have started." );
			}

			foreach ( LogDirectoryConfigurationInfo dir in e.NewItems )
			{
				AddDirectory( dir );
			}
		}

		private void OnDirectoryReadProgress( object sender, FileReadEventArgs e )
		{
			RaiseReadProgress( e );
		}

		private void OnDirectoryLoaded( object sender, EventArgs e )
		{
			Interlocked.Increment( ref _directoriesLoadedCount );
			if ( _directoriesLoadedCount == _directories.Count )
			{
				PerformInitialMerge();
				RaiseLoadedEvent();
			}

			_loadedEvent.Signal();
			LogDirectory dir = (LogDirectory)sender;
			dir.Loaded -= OnDirectoryLoaded;
		}

		protected override void StartImpl()
		{
			if ( _config.Directories.Count == 0 )
			{
				throw new InvalidOperationException( "Config should have at least one LogDirectory." );
			}
			if ( !_config.EnabledDirectories.Any() )
			{
				throw new InvalidOperationException( "Config should have at least one enabled LogDirectory." );
			}

			_loadedEvent = new CountdownEvent( _directories.Count );

			foreach ( var dir in Directories )
			{
				dir.Start();
			}
		}

		private CountdownEvent _loadedEvent;

		public void WaitForLoaded()
		{
			if ( IsLoaded )
			{
				return;
			}

			_loadedEvent.Wait();
			_loadedEvent.Dispose();
		}

		private void PerformInitialMerge()
		{
			PerformInitialMerge(
				Directories.Sum( d => d.Files.Sum( f => f.LogEntries.Count ) ),
								Directories.SelectMany( d => d.Files.SelectMany( f => f.LogEntries ) ) );
		}

		public override long TotalLengthInBytes
		{
			get { return Directories.Sum( d => d.TotalLengthInBytes ); }
		}

		public void Accept( ILogVisitor visitor )
		{
			visitor.Visit( this );
		}
	}
}
