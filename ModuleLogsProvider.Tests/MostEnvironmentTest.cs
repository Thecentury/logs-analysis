using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer;
using LogAnalyzer.Config;
using ModuleLogsProvider.Logging;
using ModuleLogsProvider.Logging.Mocks;
using ModuleLogsProvider.Logging.Most;
using NUnit.Framework;

namespace ModuleLogsProvider.Tests
{
	[TestFixture]
	public class MostEnvironmentTest
	{
		private readonly MockTimer timer = new MockTimer();
		private readonly MockLogSourceFactory serviceFactory = new MockLogSourceFactory();

		[Test]
		public void TestCreation()
		{
			LogAnalyzerConfiguration config = BuildConfig();
			MostEnvironment env = new MostEnvironment( config );
		}

		private LogAnalyzerConfiguration BuildConfig()
		{
			var config = LogAnalyzerConfiguration.Create()
							.AddLogDirectory( "Dir1", "*", "Some directory 1" )
							.AddLogWriter( new DebugLogWriter() )
							.AcceptAllLogTypes()
							.RegisterInstance<ITimer>( timer )
							.RegisterInstance<ILogSourceServiceFactory>( serviceFactory )
							.BuildConfig();

			return config;
		}
	}
}
