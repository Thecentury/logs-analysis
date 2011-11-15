using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
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
using LogAnalyzer.Tests.Mock;
using NUnit.Framework;

namespace LogAnalyzer.Tests
{
	[TestFixture]
	public class HighlightViewModelTests
	{
		private readonly ManualResetEventSlim dispatcherStartedEvent = new ManualResetEventSlim();

		[Test]
		public void TestRemoveHighlightingCommand()
		{
			var config = LogAnalyzerConfiguration.CreateNew()
				.AddLogDirectory( new LogDirectoryConfigurationInfo( "Mock", "*", "Mock" ) { EncodingName = Encoding.Unicode.WebName } )
				.SetScheduler( Scheduler.Immediate );

			var env = new MockEnvironment( config );
			MockDirectoryInfo dir = (MockDirectoryInfo)env.GetDirectory( "Mock" );
			var file = dir.AddFile( "1" );

			LogAnalyzerCore core = new LogAnalyzerCore( config, env );
			core.Start();
			core.WaitForLoaded();

			file.WriteInfo( "Message 1" );

			Thread dispatcherThread = new Thread( DispatcherProc )
										{
											IsBackground = true
										};
			dispatcherThread.Start();

			dispatcherStartedEvent.Wait();

			ApplicationViewModel appViewModel = new ApplicationViewModel( config, env );
			CoreViewModel coreViewModel = new CoreViewModel( core, appViewModel );
			HighlightingViewModel highlightingViewModel = new HighlightingViewModel( coreViewModel, new AlwaysTrue(), new SolidColorBrush() );
			coreViewModel.HighlightingFilters.Add( highlightingViewModel );

			var entryVms = coreViewModel.EntriesView.Cast<LogEntryViewModel>().ToList();
			var first = entryVms[0];

			Assert.Contains( highlightingViewModel, first.HighlightedByList );

			highlightingViewModel.RemoveHighlightingCommand.Execute();

			Assert.AreEqual( 0, first.HighlightedByList.Count );
			Assert.AreEqual( 0, coreViewModel.HighlightingFilters.Count );
		}

		private void DispatcherProc()
		{
			DispatcherHelper.RunningDispatcher = Dispatcher.CurrentDispatcher;
			dispatcherStartedEvent.Set();

			Dispatcher.Run();
		}
	}
}
