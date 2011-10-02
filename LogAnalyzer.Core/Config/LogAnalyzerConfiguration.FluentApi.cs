using System;
using System.Reactive.Concurrency;
using System.Windows.Threading;

namespace LogAnalyzer.Config
{
	public partial class LogAnalyzerConfiguration
	{
		public static LogAnalyzerConfiguration CreateNew()
		{
			return new LogAnalyzerConfiguration();
		}

		public LogAnalyzerConfiguration AddLogDirectory( LogDirectoryConfigurationInfo logDirectory )
		{
			if ( logDirectory == null ) throw new ArgumentNullException( "logDirectory" );
			directories.Add( logDirectory );

			return this;
		}

		public LogAnalyzerConfiguration AddLogDirectory( string path, string fileNameFilter, string displayName )
		{
			LogDirectoryConfigurationInfo logDirectory = new LogDirectoryConfigurationInfo( path, fileNameFilter, displayName );
			directories.Add( logDirectory );

			return this;
		}

		public LogAnalyzerConfiguration AddLoggerAcceptedMessageType( MessageType messageType )
		{
			Logger.AcceptedTypes.Add( messageType );
			return this;
		}

		public LogAnalyzerConfiguration AddLogWriter( LogWriter writer )
		{
			if ( writer == null ) throw new ArgumentNullException( "writer" );
			LoggerWriters.Add( writer );
			return this;
		}

		public LogAnalyzerConfiguration AcceptAllLogTypes()
		{
			LoggerAcceptedTypes.Add( MessageType.Debug );
			LoggerAcceptedTypes.Add( MessageType.Error );
			LoggerAcceptedTypes.Add( MessageType.Info );
			LoggerAcceptedTypes.Add( MessageType.Verbose );

			return this;
		}

		public LogAnalyzerConfiguration SetScheduler( IScheduler scheduler )
		{
			if ( scheduler == null ) throw new ArgumentNullException( "scheduler" );
			RegisterInstance<IScheduler>( scheduler );

			return this;
		}


		public LogAnalyzerConfiguration SetSchedulerFromDispatcher( Dispatcher dispatcher )
		{
			if ( dispatcher == null ) throw new ArgumentNullException( "dispatcher" );

			return SetScheduler( new DispatcherScheduler( dispatcher ) );
		}
	}
}
