using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer;
using LogAnalyzer.Config;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.Kernel;
using LogAnalyzer.Operations;
using ModuleLogsProvider.Logging;
using ModuleLogsProvider.Logging.Auxilliary;
using ModuleLogsProvider.Logging.Most;
using ModuleLogsProvider.Logging.MostLogsServices;
using ModuleLogsProvider.Tests.Auxilliary;

namespace ModuleLogsProvider.Tests
{
	public static class EnvironmentTestHelper
	{
		public static MostLogAnalyzerConfiguration BuildConfig( ITimer timer, IFactory<IDisposableService<ILogSourceService>> serviceFactory, OperationScheduler scheduler, IOperationsQueue operationsQueue )
		{
			var config = MostLogAnalyzerConfiguration.CreateNew()
				.AddLogDirectory( "Dir1", "*", "Some directory 1" )
				.AddLogWriter( new DebugLogWriter() )
				.AcceptAllLogTypes()
				.WithLogsUpdateTimer( timer );

			config
				.RegisterInstance<IFactory<IDisposableService<ILogSourceService>>>( serviceFactory )
				.RegisterInstance<OperationScheduler>( scheduler )
				.RegisterInstance<IOperationsQueue>( operationsQueue )
				.RegisterInstance<IWindowService>( new RealWindowService() )
				.RegisterInstance<IErrorReportingService>( new NullErrorReportingService() );

			return config;
		}
	}
}
