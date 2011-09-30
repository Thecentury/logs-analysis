using System;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using LogAnalyzer.GUI.ViewModels;
using LogAnalyzer.Kernel;
using LogAnalyzer.Operations;
using ModuleLogsProvider.Logging.Mocks;
using ModuleLogsProvider.Logging.Most;
using ModuleLogsProvider.Logging.MostLogsServices;
using NUnit.Framework;
using ModuleLogsProvider.Tests.Auxilliary;

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

			var firstDirectoryViewModel = appViewModel.Core.Directories[0];

			var dirViewModelMergedEntriesCollectionChanged = firstDirectoryViewModel.MergedEntries.CreateEventCounterFromCollectionChanged();

			var firstDirectory = appViewModel.Core.Directories.First();
			var dirMergedEntriesCollectionChanged = firstDirectory.MergedEntries.CreateEventCounterFromCollectionChanged();

			service.AddMessage( new LogMessageInfo { MessageType = "E", Message = "[E] [ 69] 24.05.2011 0:00:12	Message1", LoggerName = LoggerName } );
			timer.MakeRing();

			queue.WaitAllRunningOperationsToComplete();

			var firstFile = firstDirectoryViewModel.Files.First( f => f.Name == LoggerName );

			Assert.AreEqual( 1, firstFile.LogEntries.Count );
			Assert.AreEqual( 1, firstDirectoryViewModel.MergedEntries.Count );
			Assert.AreEqual( 1, dirMergedEntriesCollectionChanged.CalledTimes );
			Assert.AreEqual( 1, dirViewModelMergedEntriesCollectionChanged.CalledTimes );
		}
	}
}
