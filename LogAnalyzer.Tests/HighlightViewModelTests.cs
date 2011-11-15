using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Windows.Media;
using LogAnalyzer.Config;
using LogAnalyzer.Filters;
using LogAnalyzer.GUI.ViewModels;
using LogAnalyzer.Tests.Mock;
using NUnit.Framework;

namespace LogAnalyzer.Tests
{
	//[Timeout( 2000 )]
	[TestFixture]
	public class HighlightViewModelTests
	{
		[Test]
		public void TestRemoveHighlightingCommand()
		{
			var config = LogAnalyzerConfiguration.CreateNew()
				.AddLogDirectory( "Mock", "*", "Mock" )
				.SetScheduler( Scheduler.Immediate );

			var env = new MockEnvironment( config );
			MockDirectoryInfo dir = (MockDirectoryInfo)env.GetDirectory( "Mock" );
			var file = dir.AddFile( "1" );
			file.WriteInfo( "Message 1" );

			LogAnalyzerCore core = new LogAnalyzerCore( config, env );
			core.Start();
			core.WaitForLoaded();

			ApplicationViewModel appViewModel = new ApplicationViewModel( config, env );
			CoreViewModel coreViewModel = new CoreViewModel( core, appViewModel );
			HighlightingViewModel highlightingViewModel = new HighlightingViewModel( coreViewModel, new AlwaysTrue(), new SolidColorBrush() );
			coreViewModel.HighlightingFilters.Add( highlightingViewModel );

			var first = coreViewModel.EntriesView.GetItemAt( 0 );

			//Assert.Contains( highlightingViewModel, first.HighlightedByList );
		}
	}
}
