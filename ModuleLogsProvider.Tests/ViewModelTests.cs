using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using LogAnalyzer;
using LogAnalyzer.Config;
using LogAnalyzer.GUI.ViewModels;
using LogAnalyzer.Kernel;
using LogAnalyzer.Operations;
using ModuleLogsProvider.Logging;
using ModuleLogsProvider.Logging.Mocks;
using ModuleLogsProvider.Logging.Most;
using ModuleLogsProvider.Logging.MostLogsServices;
using NUnit.Framework;

namespace ModuleLogsProvider.Tests
{
	[TestFixture]
	public class ViewModelTests
	{
		private const string LoggerName = "L1";

		[Test]
		public void TestLogEntryAddedAfterCoreStart()
		{
			var timer = new MockTimer();
			var service = new MockLogsSourceService();
			var serverFactory = new MockLogSourceFactory( service );
			var scheduler = OperationScheduler.SyncronousScheduler;
			var queue = new SameThreadOperationsQueue();

			var config = EnvironmentTestHelper.BuildConfig( timer, serverFactory, scheduler, queue );
			config.RegisterInstance<IScheduler>( Scheduler.Immediate );

			MostEnvironment env = new MostEnvironment( config );
			config.RegisterInstance<IEnvironment>( env );

			ApplicationViewModel appViewModel = new ApplicationViewModel( config );
			appViewModel.Core.WaitForLoaded();

			Assert.That( appViewModel.CoreViewModel.Directories.Count, Is.EqualTo( 1 ) );

			service.AddMessage( new LogMessageInfo { MessageType = "E", Message = "[E] [ 69] 24.05.2011 0:00:12	Message1", LoggerName = LoggerName } );
			timer.MakeRing();

			queue.WaitAllRunningOperationsToComplete();

			var firstDirectoryViewModel = appViewModel.Core.Directories[0];
			var firstFile = firstDirectoryViewModel.Files.FirstOrDefault( f => f.Name == LoggerName );
			Assert.That( firstFile, Is.Not.Null );
		}
	}
}
