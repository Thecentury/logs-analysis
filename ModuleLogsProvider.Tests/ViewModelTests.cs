using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer;
using LogAnalyzer.Config;
using LogAnalyzer.GUI.ViewModels;
using LogAnalyzer.Kernel;
using LogAnalyzer.Operations;
using ModuleLogsProvider.Logging;
using ModuleLogsProvider.Logging.Mocks;
using ModuleLogsProvider.Logging.Most;
using NUnit.Framework;

namespace ModuleLogsProvider.Tests
{
	[TestFixture]
	public class ViewModelTests
	{
		[Test]
		public void TestLogEntryAddedAfterCoreStart()
		{
			var timer = new MockTimer();
			var serverFactory = new MostServerLogSourceFactory();
			var scheduler = OperationScheduler.TaskScheduler;
			var queue = new WorkerThreadOperationsQueue();

			var config = EnvironmentTestHelper.BuildConfig( timer, serverFactory, scheduler, queue );

			MostEnvironment env = new MostEnvironment( config );
			config.RegisterInstance<IEnvironment>( env );


			ApplicationViewModel appViewModel = new ApplicationViewModel( config );
			appViewModel.Core.WaitForLoaded();

			Assert.That( appViewModel.CoreViewModel.Directories.Count, Is.EqualTo( 1 ) );
		}
	}
}
