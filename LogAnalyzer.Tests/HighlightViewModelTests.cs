using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using LogAnalyzer.Config;
using LogAnalyzer.Extensions;
using LogAnalyzer.Filters;
using LogAnalyzer.GUI.Extensions;
using LogAnalyzer.GUI.ViewModels;
using LogAnalyzer.Tests.Common;
using LogAnalyzer.Tests.Mocks;
using NUnit.Framework;

namespace LogAnalyzer.Tests
{
	[TestFixture]
	public class HighlightViewModelTests
	{
		private ManualResetEventSlim dispatcherStartedEvent;
		private LogAnalyzerConfiguration config;
		private Thread dispatcherThread;
		private ApplicationViewModel appViewModel;
		private CoreViewModel coreViewModel;
		private MockDirectoryInfo dir;
		private MockFileInfo file1;

		[SetUp]
		public void Setup()
		{
			dispatcherStartedEvent = new ManualResetEventSlim();
			dispatcherThread = new Thread( DispatcherProc )
			{
				IsBackground = true
			};
			dispatcherThread.Start();

			dispatcherStartedEvent.Wait();

			config = LogAnalyzerConfiguration.CreateNew()
				.AddLogDirectory( new LogDirectoryConfigurationInfo( "Mock", "*", "Mock" ) { EncodingName = Encoding.Unicode.WebName } )
				.SetScheduler( new DispatcherScheduler( DispatcherHelper.RunningDispatcher ) );

			var env = new MockEnvironment( config );
			dir = (MockDirectoryInfo)env.GetDirectory( "Mock" );
			file1 = dir.AddFile( "1" );

			LogAnalyzerCore core = new LogAnalyzerCore( config, env );
			core.Start();
			core.WaitForLoaded();


			appViewModel = new ApplicationViewModel( config, env );
			coreViewModel = new CoreViewModel( core, appViewModel );
		}

		[TearDown]
		public void TearDown()
		{
			DispatcherHelper.RunningDispatcher.InvokeShutdown();
		}

		private void DispatcherProc()
		{
			DispatcherHelper.RunningDispatcher = Dispatcher.CurrentDispatcher;
			dispatcherStartedEvent.Set();

			Dispatcher.Run();
		}

		[Test]
		public void TestRemoveHighlightingCommand()
		{
			file1.WriteInfo( "Message 1" );
			Thread.Sleep( 100 );

			HighlightingViewModel highlightingViewModel = new HighlightingViewModel( coreViewModel, new AlwaysTrue() );
			coreViewModel.HighlightingFilters.Add( highlightingViewModel );

			var entryVms = coreViewModel.EntriesView.Cast<LogEntryViewModel>().ToList();
			var first = entryVms[0];

			Assert.Contains( highlightingViewModel, first.HighlightedByList );

			highlightingViewModel.RemoveHighlightingCommand.Execute();

			Assert.AreEqual( 0, first.HighlightedByList.Count );
			Assert.AreEqual( 0, coreViewModel.HighlightingFilters.Count );
		}

		[Test]
		public void TestMoveToFirstOrLastHighlightedCommand()
		{
			HighlightingViewModel hvm = new HighlightingViewModel( coreViewModel, new AlwaysTrue() );

			Assert.IsFalse( hvm.MoveToFirstHighlightedCommand.CanExecute() );
			Assert.IsFalse( hvm.MoveToLastHighlightedCommand.CanExecute() );

			var observable = coreViewModel.EntriesView.ToNotifyCollectionChangedObservable();
			using ( observable.WaitForCount( 1 ) )
			{
				file1.WriteInfo( "Message 1" );
			}
			Thread.Sleep( 100 );

			Assert.IsTrue( hvm.MoveToFirstHighlightedCommand.CanExecute() );
			Assert.IsTrue( hvm.MoveToLastHighlightedCommand.CanExecute() );

			hvm.MoveToFirstHighlightedCommand.Execute();

			using ( observable.WaitForCount( 1 ) )
			{
				file1.WriteInfo( "Message 2" );
			}
			Thread.Sleep( 100 );

			Assert.IsFalse( hvm.MoveToFirstHighlightedCommand.CanExecute() );
			Assert.IsTrue( hvm.MoveToLastHighlightedCommand.CanExecute() );
			Assert.That( coreViewModel.SelectedEntryIndex, Is.EqualTo( 0 ) );

			hvm.MoveToLastHighlightedCommand.Execute();

			Assert.IsTrue( hvm.MoveToFirstHighlightedCommand.CanExecute() );
			Assert.IsFalse( hvm.MoveToLastHighlightedCommand.CanExecute() );
			Assert.That( coreViewModel.SelectedEntryIndex, Is.EqualTo( 1 ) );

			hvm.MoveToFirstHighlightedCommand.Execute();

			Assert.IsFalse( hvm.MoveToFirstHighlightedCommand.CanExecute() );
			Assert.IsTrue( hvm.MoveToLastHighlightedCommand.CanExecute() );
			Assert.That( coreViewModel.SelectedEntryIndex, Is.EqualTo( 0 ) );
		}

		[Test]
		public void TestMoveToNextOrPrevHighlighted()
		{
			HighlightingViewModel hvm = new HighlightingViewModel( coreViewModel, new AlwaysTrue() );

			Assert.IsFalse( hvm.MoveToPreviousHighlightedCommand.CanExecute() );
			Assert.IsFalse( hvm.MoveToNextHighlightedCommand.CanExecute() );

			var observable = coreViewModel.EntriesView.ToNotifyCollectionChangedObservable();

			using ( observable.WaitForCount( 3 ) )
			{
				file1.WriteInfo( "Message1" );
				file1.WriteInfo( "Message2" );
				file1.WriteInfo( "Message3" );
			}
			Thread.Sleep( 100 );

			hvm.MoveToNextHighlightedCommand.CanExecute().AssertIsTrue();
			hvm.MoveToPreviousHighlightedCommand.CanExecute().AssertIsFalse();

			hvm.MoveToNextHighlightedCommand.Execute();

			hvm.MoveToNextHighlightedCommand.CanExecute().AssertIsTrue();
			// we are at 0th element
			hvm.MoveToPreviousHighlightedCommand.CanExecute().AssertIsFalse();

			hvm.MoveToNextHighlightedCommand.Execute();

			hvm.MoveToNextHighlightedCommand.CanExecute().AssertIsTrue();
			hvm.MoveToPreviousHighlightedCommand.CanExecute().AssertIsTrue();
		}
	}
}
