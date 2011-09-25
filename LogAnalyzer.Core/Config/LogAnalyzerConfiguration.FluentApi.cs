using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Config;

namespace LogAnalyzer
{
	public class LogAnalyzerConfigurationFluentApi
	{
		private readonly LogAnalyzerConfiguration config;

		public LogAnalyzerConfiguration BuildConfig()
		{
			return config;
		}

		internal LogAnalyzerConfigurationFluentApi( LogAnalyzerConfiguration config )
		{
			if ( config == null )
				throw new ArgumentNullException( "config" );

			this.config = config;
		}

		public LogAnalyzerConfigurationFluentApi AddLogDirectory( LogDirectoryConfigurationInfo logDirectory )
		{
			config.Directories.Add( logDirectory );
			return this;
		}

		public LogDirectoryConfigurationInfoFluentApi AddLogDirectory( string path, string fileNameFilter, string displayName )
		{
			LogDirectoryConfigurationInfo logDirectory = new LogDirectoryConfigurationInfo( path, fileNameFilter, displayName );
			config.Directories.Add( logDirectory );
			return new LogDirectoryConfigurationInfoFluentApi( logDirectory, this.config );
		}

		public LogAnalyzerConfigurationFluentApi AddAcceptedType( MessageType messageType )
		{
			config.Logger.AcceptedTypes.Add( messageType );
			return this;
		}

		public LogAnalyzerConfigurationFluentApi AddLogWriter( LogWriter writer )
		{
			config.LoggerWriters.Add( writer );
			return this;
		}

		public LogAnalyzerConfigurationFluentApi AcceptAllLogTypes()
		{
			config.LoggerAcceptedTypes.Add( MessageType.Debug );
			config.LoggerAcceptedTypes.Add( MessageType.Error );
			config.LoggerAcceptedTypes.Add( MessageType.Info );
			config.LoggerAcceptedTypes.Add( MessageType.Verbose );

			return this;
		}

		public LogAnalyzerConfigurationFluentApi Register<TContract>( Func<object> createFunc )
		{
			config.Register<TContract>( createFunc );

			return this;
		}

		public LogAnalyzerConfigurationFluentApi RegisterInstance<TContract>( TContract instance )
		{
			config.RegisterInstance<TContract>( instance );

			return this;
		}
	}
}
