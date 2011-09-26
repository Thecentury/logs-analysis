using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LogAnalyzer;
using LogAnalyzer.Config;
using ModuleLogsProvider.Logging;
using ModuleLogsProvider.Logging.Mocks;
using ModuleLogsProvider.Logging.Most;
using ModuleLogsProvider.Logging.MostLogsServices;
using NUnit.Framework;

namespace ModuleLogsProvider.Tests
{
	[TestFixture]
	public class MostEnvironmentTest
	{
		private readonly MockTimer timer = new MockTimer();
		private readonly MockLogsSourceService service = new MockLogsSourceService();
		private MockLogSourceFactory serviceFactory;

		[Test]
		public void TestCreation()
		{
			serviceFactory = new MockLogSourceFactory( service );
			service.AddMessage( new LogMessageInfo { MessageType = "E", Message = "[E] [ 69] 24.05.2011 0:00:12	Message1", LoggerName = "L1" } );

			LogAnalyzerConfiguration config = BuildConfig();
			MostEnvironment env = new MostEnvironment( config );

			LogAnalyzerCore core = new LogAnalyzerCore( config, env );
			core.Start();
			core.WaitForLoaded();

			Task task = Task.Factory.StartNew( () =>
												{
													timer.MakeRing();
												} );

			Thread.Sleep( 2000 );
			core.OperationsQueue.WaitAllRunningOperationsToComplete();
			task.Wait();

			Assert.That( core.Directories.Count, Is.EqualTo( 1 ) );

			var firstDir = core.Directories[0];
			Assert.That( firstDir, Is.Not.Null );
			ExpressionAssert.That( firstDir, d => d.Files.Count == 1 );
			ExpressionAssert.That( firstDir, d => d.Files.Count != 1 );
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
