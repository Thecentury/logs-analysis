using System;
using System.Reactive.Concurrency;
using System.Windows.Threading;
using LogAnalyzer.Extensions;
using LogAnalyzer.Logging;

namespace LogAnalyzer.Config
{
	public static class LogAnalyzerConfigurationExtensions
	{
		public static T AddLogDirectory<T>( this T config, LogDirectoryConfigurationInfo logDirectory ) where T : LogAnalyzerConfiguration
		{
			if ( logDirectory == null ) throw new ArgumentNullException( "logDirectory" );
			config.Directories.Add( logDirectory );

			return config;
		}

		public static T AddLogDirectory<T>( this T config, string path, string fileNameFilter, string displayName ) where T : LogAnalyzerConfiguration
		{
			LogDirectoryConfigurationInfo logDirectory = new LogDirectoryConfigurationInfo( path, fileNameFilter, displayName );
			config.Directories.Add( logDirectory );

			return config;
		}

		public static T AddLoggerAcceptedMessageType<T>( this T config, MessageType messageType ) where T : LogAnalyzerConfiguration
		{
			config.Logger.AcceptedTypes.Add( messageType );
			return config;
		}

		public static T AddLogWriter<T>( this T config, LogWriter writer ) where T : LogAnalyzerConfiguration
		{
			if ( writer == null ) throw new ArgumentNullException( "writer" );
			config.LoggerWriters.Add( writer );
			return config;
		}

		public static T AcceptAllLogTypes<T>( this T config ) where T : LogAnalyzerConfiguration
		{
			config.LoggerAcceptedTypes.Add( MessageType.Error );
			config.LoggerAcceptedTypes.Add( MessageType.Warning );
			config.LoggerAcceptedTypes.Add( MessageType.Info );
			config.LoggerAcceptedTypes.Add( MessageType.Debug );
			config.LoggerAcceptedTypes.Add( MessageType.Verbose );

			return config;
		}

		public static T SetScheduler<T>( this T config, IScheduler scheduler ) where T : LogAnalyzerConfiguration
		{
			if ( scheduler == null ) throw new ArgumentNullException( "scheduler" );
			config.Container.RegisterInstance<IScheduler>( scheduler );

			return config;
		}

		public static T SetSchedulerFromDispatcher<T>( this T config, Dispatcher dispatcher ) where T : LogAnalyzerConfiguration
		{
			if ( dispatcher == null ) throw new ArgumentNullException( "dispatcher" );

			return config.SetScheduler( new DispatcherScheduler( dispatcher ) );
		}

		public static LogAnalyzerConfiguration Register<TContract>( this LogAnalyzerConfiguration config, Func<object> createInstanceFunc )
		{
			config.Container.Register<TContract>( createInstanceFunc );

			return config;
		}

		public static LogAnalyzerConfiguration RegisterInstance<T>( this LogAnalyzerConfiguration config, object instance )
		{
			config.Container.RegisterInstance<T>( instance );

			return config;
		}
	}
}
