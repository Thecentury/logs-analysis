using System;
using LogAnalyzer;
using LogAnalyzer.Config;
using LogAnalyzer.Kernel;

namespace ModuleLogsProvider.Logging.Most
{
	public sealed class MostEnvironment : EnvironmentBase
	{
		private readonly ITimeService timeService = new ConstIntervalTimeService( TimeSpan.FromDays( 1000 ) );
		private readonly WorkerThreadOperationsQueue operationsQueue;
		private readonly MostDirectoryInfo directory;

		public MostEnvironment( LogAnalyzerConfiguration config )
			: base( config )
		{
			if ( config == null )
				throw new ArgumentNullException( "config" );

			this.operationsQueue = new WorkerThreadOperationsQueue( config.Logger );

			ILogSourceServiceFactory serviceFactory = config.Resolve<ILogSourceServiceFactory>();
			if ( serviceFactory == null ) 
				throw new ArgumentException( "serviceFactory" );

			ITimer timer = config.Resolve<ITimer>();
			if ( timer == null ) 
				throw new ArgumentException( "timer" );

			MostNotificationSource notificationSource = new MostNotificationSource( timer, serviceFactory, operationsQueue );
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
