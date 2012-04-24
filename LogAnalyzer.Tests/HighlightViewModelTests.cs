using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using LogAnalyzer.GUI.ViewModels.Colorizing;
using LogAnalyzer.Tests.Common;
using LogAnalyzer.Tests.Mocks;
using NUnit.Framework;

namespace LogAnalyzer.Tests
{
	[TestFixture]
	public class HighlightViewModelTests
	{
		private ManualResetEventSlim _dispatcherStartedEvent;
		private LogAnalyzerConfiguration _config;
		private Thread _dispatcherThread;
		private ApplicationViewModel _appViewModel;
		private CoreViewModel _coreViewModel;
		private MockDirectoryInfo _dir;
		private MockFileInfo _file1;

		[SetUp]
		public void Setup()
		{
			_dispatcherStartedEvent = new ManualResetEventSlim();
			_dispatcherThread = new Thread( DispatcherProc )
			{
				IsBackground = true
			};
			_dispatcherThread.Start();

			_dispatcherStartedEvent.Wait();

			_config = LogAnalyzerConfiguration.CreateNew()
				.AddLogDirectory( new LogDirectoryConfigurationInfo( "Mock", "*", "Mock" ) { EncodingName = Encoding.Unicode.WebName } )
				.SetScheduler( new DispatcherScheduler( DispatcherHelper.RunningDispatcher ) )
				.RegisterInstance( new ColorizationManager( new List<ColorizeTemplateBase>() ) );

			var env = new MockEnvironment( _config );
			_dir = (MockDirectoryInfo)env.GetDirectory( "Mock" );
			_file1 = _dir.AddFile( "1" );

			LogAnalyzerCore core = new LogAnalyzerCore( _config, env );
			core.Start();
			core.WaitForLoaded();

			_appViewModel = new ApplicationViewModel( _config, env );
			_coreViewModel = new CoreViewModel( core, _appViewModel );
		}

		[TearDown]
		public void TearDown()
		{
			DispatcherHelper.RunningDispatcher.InvokeShutdown();
		}

		private void DispatcherProc()
		{
			DispatcherHelper.RunningDispatcher = Dispatcher.CurrentDispatcher;
			_dispatcherStartedEvent.Set();

			Dispatcher.Run();
		}

		[Test]
		public void TestRemoveHighlightingCommand()
		{
			INotifyCollectionChanged observableCollection =
				_coreViewModel.EntriesView.SourceCollection as INotifyCollectionChanged;

			ManualResetEventSlim entryAddedAwaiter = new ManualResetEventSlim();
			observableCollection.ToNotifyCollectionChangedObservable().Subscribe( e =>
				{
					entryAddedAwaiter.Set();
				} );

			_file1.WriteInfo( "Message 1" );

			entryAddedAwaiter.Wait();

			HighlightingViewModel highlightingViewModel = new HighlightingViewModel( _coreViewModel, new AlwaysTrue() );
			_coreViewModel.HighlightingFilters.Add( highlightingViewModel );

			var entryVms = _coreViewModel.EntriesView.Cast<LogEntryViewModel>().ToList();
			var first = entryVms[0];

			Assert.Contains( highlightingViewModel, first.HighlightedByList );

			highlightingViewModel.RemoveHighlightingCommand.Execute();

			Assert.AreEqual( 0, first.HighlightedByList.Count );
			Assert.AreEqual( 0, _coreViewModel.HighlightingFilters.Count );
		}

		[Test]
		public void TestMoveToFirstOrLastHighlightedCommand()
		{
			HighlightingViewModel hvm = new HighlightingViewModel( _coreViewModel, new AlwaysTrue() );

			Assert.IsFalse( hvm.MoveToFirstHighlightedCommand.CanExecute() );
			Assert.IsFalse( hvm.MoveToLastHighlightedCommand.CanExecute() );

			var observable = _coreViewModel.EntriesView.ToNotifyCollectionChangedObservable();
			using ( observable.WaitForCount( 1 ) )
			{
				_file1.WriteInfo( "Message 1" );
			}
			Thread.Sleep( 100 );

			Assert.IsTrue( hvm.MoveToFirstHighlightedCommand.CanExecute() );
			Assert.IsTrue( hvm.MoveToLastHighlightedCommand.CanExecute() );

			hvm.MoveToFirstHighlightedCommand.Execute();

			using ( observable.WaitForCount( 1 ) )
			{
				_file1.WriteInfo( "Message 2" );
			}
			Thread.Sleep( 100 );

			Assert.IsFalse( hvm.MoveToFirstHighlightedCommand.CanExecute() );
			Assert.IsTrue( hvm.MoveToLastHighlightedCommand.CanExecute() );
			Assert.That( _coreViewModel.SelectedEntryIndex, Is.EqualTo( 0 ) );

			hvm.MoveToLastHighlightedCommand.Execute();

			Assert.IsTrue( hvm.MoveToFirstHighlightedCommand.CanExecute() );
			Assert.IsFalse( hvm.MoveToLastHighlightedCommand.CanExecute() );
			Assert.That( _coreViewModel.SelectedEntryIndex, Is.EqualTo( 1 ) );

			hvm.MoveToFirstHighlightedCommand.Execute();

			Assert.IsFalse( hvm.MoveToFirstHighlightedCommand.CanExecute() );
			Assert.IsTrue( hvm.MoveToLastHighlightedCommand.CanExecute() );
			Assert.That( _coreViewModel.SelectedEntryIndex, Is.EqualTo( 0 ) );
		}

		[Test]
		public void TestMoveToNextOrPrevHighlighted()
		{
			HighlightingViewModel hvm = new HighlightingViewModel( _coreViewModel, new AlwaysTrue() );

			Assert.IsFalse( hvm.MoveToPreviousHighlightedCommand.CanExecute() );
			Assert.IsFalse( hvm.MoveToNextHighlightedCommand.CanExecute() );

			var observable = _coreViewModel.EntriesView.ToNotifyCollectionChangedObservable();

			using ( observable.WaitForCount( 3 ) )
			{
				_file1.WriteInfo( "Message1" );
				_file1.WriteInfo( "Message2" );
				_file1.WriteInfo( "Message3" );
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
