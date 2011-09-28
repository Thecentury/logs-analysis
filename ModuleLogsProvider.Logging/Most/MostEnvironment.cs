using System;
using LogAnalyzer;
using LogAnalyzer.Config;
using LogAnalyzer.Kernel;

namespace ModuleLogsProvider.Logging.Most
{
	public sealed class MostEnvironment : EnvironmentBase
	{
		private readonly ITimeService timeService = new ConstIntervalTimeService( TimeSpan.FromDays( 1000 ) );
		private readonly IOperationsQueue operationsQueue;
		private readonly MostDirectoryInfo directory;

		public MostEnvironment( LogAnalyzerConfiguration config )
			: base( config )
		{
			if ( config == null )
				throw new ArgumentNullException( "config" );

			operationsQueue = config.ResolveNotNull<IOperationsQueue>();

			ILogSourceServiceFactory serviceFactory = config.ResolveNotNull<ILogSourceServiceFactory>();
			ITimer timer = config.ResolveNotNull<ITimer>();

			MostLogNotificationSource notificationSource = new MostLogNotificationSource( timer, serviceFactory, operationsQueue );
			directory = new MostDirectoryInfo( notificationSource );
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
