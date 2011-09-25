using System;
using LogAnalyzer;
using ModuleLogsProvider.Logging.Mocks;

namespace ModuleLogsProvider.Logging.Most
{
	public sealed class MostEnvironment : IEnvironment
	{
		private readonly ITimeService timeService = new ConstIntervalTimeService( TimeSpan.FromDays( 1000 ) );
		private readonly WorkerThreadOperationsQueue operationsQueue = null;
		private readonly MostDirectoryInfo directory;

		public MostEnvironment( LogAnalyzerConfiguration config )
		{
			if ( config == null )
				throw new ArgumentNullException( "config" );

			this.operationsQueue = new WorkerThreadOperationsQueue( config.Logger );

			ILogSourceServiceFactory serviceFactory = new MockLogSourceFactory();

			// todo brinchuk move update interval to config
			ThreadingTimer timer = new ThreadingTimer( TimeSpan.FromSeconds( 20 ) );

			MostNotificationSource notificationSource = new MostNotificationSource( timer, serviceFactory );
			directory = new MostDirectoryInfo( notificationSource );
		}

		public IDirectoryInfo GetDirectory( string path )
		{
			return directory;
		}

		public IOperationsQueue OperationsQueue
		{
			get { return operationsQueue; }
		}

		public ITimeService TimeService
		{
			get { return timeService; }
		}
	}
}
