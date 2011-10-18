using LogAnalyzer;
using LogAnalyzer.Config;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.Kernel;
using LogAnalyzer.Operations;
using ModuleLogsProvider.Logging;
using ModuleLogsProvider.Logging.MostLogsServices;

namespace ModuleLogsProvider.Tests
{
	public static class EnvironmentTestHelper
	{
		public static MostLogAnalyzerConfiguration BuildConfig( ITimer timer, IServiceFactory<ILogSourceService> serviceFactory, OperationScheduler scheduler, IOperationsQueue operationsQueue )
		{
			var config = MostLogAnalyzerConfiguration.CreateNew()
				.AddLogDirectory( "Dir1", "*", "Some directory 1" )
				.AddLogWriter( new DebugLogWriter() )
				.AcceptAllLogTypes()
				.WithLogsUpdateTimer( timer );

			config
				.RegisterInstance<IServiceFactory<ILogSourceService>>( serviceFactory )
				.RegisterInstance<OperationScheduler>( scheduler )
				.RegisterInstance<IOperationsQueue>( operationsQueue )
				.RegisterInstance<IWindowService>( new RealWindowService() )
				.RegisterInstance<ErrorReportingServiceBase>( new NullErrorReportingService() );

			return config;
		}
	}
}
