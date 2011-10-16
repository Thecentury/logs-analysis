using System;
using LogAnalyzer;
using LogAnalyzer.Config;
using LogAnalyzer.Extensions;
using LogAnalyzer.Kernel;
using ModuleLogsProvider.Logging.Auxilliary;
using ModuleLogsProvider.Logging.MostLogsServices;

namespace ModuleLogsProvider.Logging.Most
{
	public sealed class MostEnvironment : EnvironmentBase
	{
		private readonly ITimeService timeService = new ConstIntervalTimeService( TimeSpan.FromDays( 1000 ) );
		private readonly IOperationsQueue operationsQueue;
		private readonly MostDirectoryInfo directory;

		public MostEnvironment( MostLogAnalyzerConfiguration config )
			: base( config )
		{
			if ( config == null )
				throw new ArgumentNullException( "config" );

			operationsQueue = config.ResolveNotNull<IOperationsQueue>();
			IServiceFactory<ILogSourceService> serviceFactory = config.ResolveNotNull<IServiceFactory<ILogSourceService>>();
			ITimer logsUpdateTimer = config.LogsUpdateTimer;
			IErrorReportingService errorReportingService = config.ResolveNotNull<IErrorReportingService>();

			MostLogNotificationSource notificationSource = new MostLogNotificationSource( logsUpdateTimer, serviceFactory,
				operationsQueue, errorReportingService );

			directory = notificationSource.DirectoryInfo;
		}

		public override IDirectoryInfo GetDirectory( string path )
		{
			return directory;
		}

		public override IOperationsQueue OperationsQueue
		{
			get { return operationsQueue; }
		}

		public override ITimeService TimeService
		{
			get { return timeService; }
		}
	}
}
