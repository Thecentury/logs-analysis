using System;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Awad.Eticket.ModuleLogsProvider.Types;
using LogAnalyzer;
using LogAnalyzer.Config;
using LogAnalyzer.GUI.ViewModels;
using LogAnalyzer.Kernel;
using LogAnalyzer.Operations;
using LogAnalyzer.Tests;
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
		private const string LoggerName1 = "L1";
		private const string LoggerName2 = "L2";

		private readonly MockTimer timer = new MockTimer();
		private readonly MockLogsSourceService service = new MockLogsSourceService();
		private readonly SameThreadOperationsQueue queue = new SameThreadOperationsQueue();

		private LogAnalyzerConfiguration config;

		[Test]
		public void TestLogEntryAddedAfterCoreStart()
		{
			MostEnvironment env = new MostEnvironment( config );
			ApplicationViewModel appViewModel = new ApplicationViewModel( config, env );
			appViewModel.Core.WaitForLoaded();

			Assert.That( appViewModel.CoreViewModel.Directories.Count, Is.EqualTo( 1 ) );

			var firstDirectoryViewModel = appViewModel.Core.Directories[0];

			var dirViewModelMergedEntriesCollectionChanged = firstDirectoryViewModel.MergedEntries.CreateEventCounterFromCollectionChanged();

			var firstDirectory = appViewModel.Core.Directories.First();
			var dirMergedEntriesCollectionChanged = firstDirectory.MergedEntries.CreateEventCounterFromCollectionChanged();

			var coreMergedEntriesCollectionChanged = appViewModel.Core.MergedEntries.CreateEventCounterFromCollectionChanged();
			var coreViewModelEntriesCollectionChanged =
				appViewModel.CoreViewModel.EntriesView.CreateEventCounterFromCollectionChanged();

			var mergedEntriesGroupEventCounter = new CompositeEventCounter( dirMergedEntriesCollectionChanged,
																		   dirViewModelMergedEntriesCollectionChanged,
																		   coreMergedEntriesCollectionChanged,
																		   coreViewModelEntriesCollectionChanged );

			var firstDirFilesCollectionChangedEventCounter = firstDirectory.Files.CreateEventCounterFromCollectionChanged();
			var firstDirViewModelFilesCollectionChangedEventCounter =
				firstDirectoryViewModel.Files.CreateEventCounterFromCollectionChanged();

			var fileChangedGroupEventCounter = new CompositeEventCounter( firstDirFilesCollectionChangedEventCounter,
																		 firstDirViewModelFilesCollectionChangedEventCounter );

			service.AddMessage( new LogMessageInfo { MessageType = "E", Message = "[E] [ 69] 24.05.2011 0:00:12	Message1", LoggerName = LoggerName1 } );
			timer.Invoke();

			queue.WaitAllRunningOperationsToComplete();

			var firstFile = firstDirectoryViewModel.Files.First( f => f.Name == LoggerName1 );

			Assert.AreEqual( 1, firstFile.LogEntries.Count );
			Assert.AreEqual( 1, firstDirectoryViewModel.MergedEntries.Count );

			Assert.IsTrue( fileChangedGroupEventCounter.HaveBeenInvokedOneTime() );

			Assert.IsTrue( mergedEntriesGroupEventCounter.HaveBeenInvokedOneTime() );
			Assert.AreEqual( 1, dirMergedEntriesCollectionChanged.CalledTimes );
			Assert.AreEqual( 1, dirViewModelMergedEntriesCollectionChanged.CalledTimes );

			service.AddMessage( new LogMessageInfo { MessageType = "E", Message = "[E] [ 69] 24.05.2011 0:00:13	Message2", LoggerName = LoggerName1 } );
			timer.Invoke();

			queue.WaitAllRunningOperationsToComplete();

			Assert.AreEqual( 2, firstFile.LogEntries.Count );
			Assert.AreEqual( 2, firstDirectoryViewModel.MergedEntries.Count );

			Assert.IsTrue( fileChangedGroupEventCounter.HaveNotBeenInvoked() );

			Assert.IsTrue( mergedEntriesGroupEventCounter.HaveBeenInvokedOneTime() );
			Assert.AreEqual( 2, dirMergedEntriesCollectionChanged.CalledTimes );
			Assert.AreEqual( 2, dirViewModelMergedEntriesCollectionChanged.CalledTimes );

			service.AddMessage( new LogMessageInfo { MessageType = "E", Message = "[E] [ 69] 24.05.2011 0:00:13	Message3", LoggerName = LoggerName2 } );
			timer.Invoke();

			queue.WaitAllRunningOperationsToComplete();

			var secondFile = firstDirectoryViewModel.Files.First( f => f.Name == LoggerName2 );

			Assert.AreEqual( 2, firstFile.LogEntries.Count );
			Assert.AreEqual( 1, secondFile.LogEntries.Count );
			Assert.AreEqual( 3, firstDirectoryViewModel.MergedEntries.Count );

			Assert.IsTrue( fileChangedGroupEventCounter.HaveBeenInvokedOneTime() );

			Assert.IsTrue( mergedEntriesGroupEventCounter.HaveBeenInvokedOneTime() );
			Assert.AreEqual( 3, dirMergedEntriesCollectionChanged.CalledTimes );
			Assert.AreEqual( 3, dirViewModelMergedEntriesCollectionChanged.CalledTimes );
		}

		[TestFixtureSetUp]
		public void Init()
		{
			var serverFactory = new MockLogSourceFactory( service );
			var scheduler = OperationScheduler.SyncronousScheduler;

			this.config = EnvironmentTestHelper.BuildConfig( timer, serverFactory, scheduler, queue );
			config.RegisterInstance<IScheduler>( Scheduler.Immediate );
		}
	}
}
