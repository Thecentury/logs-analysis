using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using LogAnalyzer;
using ModuleLogsProvider.Logging.MostLogsServices;

namespace ModuleLogsProvider.Logging.Most
{
	public sealed class MostNotificationSource : LogNotificationsSourceBase
	{
		private const string DirectoryName = "MOST";

		private readonly List<LogMessageInfo> loadedMessages = new List<LogMessageInfo>();
		private readonly ILogSourceServiceFactory serviceFactory;
		private readonly IOperationsQueue operationQueue;

		public MostNotificationSource( ITimer timer, ILogSourceServiceFactory serviceFactory, IOperationsQueue operationQueue )
		{
			if ( timer == null ) throw new ArgumentNullException( "timer" );
			if ( serviceFactory == null ) throw new ArgumentNullException( "serviceFactory" );
			if ( operationQueue == null ) throw new ArgumentNullException( "operationQueue" );

			this.serviceFactory = serviceFactory;
			this.operationQueue = operationQueue;

			timer.Tick += OnTimerTick;
		}

		private void OnTimerTick( object sender, EventArgs e )
		{
			operationQueue.EnqueueOperation( UpdateLogMessages );
		}

		public void UpdateLogMessages()
		{
			using ( var clientWrapper = serviceFactory.CreateObject() )
			{
				var client = clientWrapper.Inner;

				int startingIndex = loadedMessages.Count;

				// todo brinchuk try-catch?
				var newMessages = client.GetMessages( startingIndex );
				NotifyOnNewMessages( newMessages );

				loadedMessages.AddRange( newMessages );
			}
		}

		private void NotifyOnNewMessages( IEnumerable<LogMessageInfo> newMessages )
		{
			var groups = newMessages.GroupBy( m => m.LoggerName );

			foreach ( var group in groups )
			{
				string loggerName = group.Key;
				RaiseChanged( new FileSystemEventArgs( WatcherChangeTypes.Changed | WatcherChangeTypes.Created,
					DirectoryName, loggerName ) );
			}
		}
	}
}
