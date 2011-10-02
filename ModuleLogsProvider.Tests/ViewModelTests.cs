﻿using System;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
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
		private const string LoggerName = "L1";

		private readonly MockTimer timer = new MockTimer();
		private readonly MockLogsSourceService service = new MockLogsSourceService();
		private readonly SameThreadOperationsQueue queue = new SameThreadOperationsQueue();

		private LogAnalyzerConfiguration config;

		[Test]
		public void TestLogEntryAddedAfterCoreStart()
		{
			ApplicationViewModel appViewModel = new ApplicationViewModel( config );
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

			var fileChangedFroupEventCounter = new CompositeEventCounter( firstDirFilesCollectionChangedEventCounter,
																		 firstDirViewModelFilesCollectionChangedEventCounter );

			service.AddMessage( new LogMessageInfo { MessageType = "E", Message = "[E] [ 69] 24.05.2011 0:00:12	Message1", LoggerName = LoggerName } );
			timer.MakeRing();

			queue.WaitAllRunningOperationsToComplete();

			var firstFile = firstDirectoryViewModel.Files.First( f => f.Name == LoggerName );

			Assert.AreEqual( 1, firstFile.LogEntries.Count );
			Assert.AreEqual( 1, firstDirectoryViewModel.MergedEntries.Count );

			Assert.IsTrue( fileChangedFroupEventCounter.HaveBeenInvokedOneTime() );

			Assert.IsTrue( mergedEntriesGroupEventCounter.HaveBeenInvokedOneTime() );
			Assert.AreEqual( 1, dirMergedEntriesCollectionChanged.CalledTimes );
			Assert.AreEqual( 1, dirViewModelMergedEntriesCollectionChanged.CalledTimes );

			service.AddMessage( new LogMessageInfo { MessageType = "E", Message = "[E] [ 69] 24.05.2011 0:00:13	Message2", LoggerName = LoggerName } );
			timer.MakeRing();

			queue.WaitAllRunningOperationsToComplete();

			Assert.AreEqual( 2, firstFile.LogEntries.Count );
			Assert.AreEqual( 2, firstDirectoryViewModel.MergedEntries.Count );

			Assert.IsTrue( fileChangedFroupEventCounter.HaveNotBeenInvoked() );

			Assert.IsTrue( mergedEntriesGroupEventCounter.HaveBeenInvokedOneTime() );
			Assert.AreEqual( 2, dirMergedEntriesCollectionChanged.CalledTimes );
			Assert.AreEqual( 2, dirViewModelMergedEntriesCollectionChanged.CalledTimes );
		}

		[TestFixtureSetUp]
		public void Init()
		{
			var serverFactory = new MockLogSourceFactory( service );
			var scheduler = OperationScheduler.SyncronousScheduler;

			this.config = EnvironmentTestHelper.BuildConfig( timer, serverFactory, scheduler, queue );
			config.RegisterInstance<IScheduler>( Scheduler.Immediate );

			MostEnvironment env = new MostEnvironment( config );
			config.RegisterInstance<IEnvironment>( env );
		}
	}
}
