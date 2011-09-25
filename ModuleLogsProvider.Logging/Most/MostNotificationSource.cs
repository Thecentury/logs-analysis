using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Awad.Eticket.ModuleLogsProvider.Types;
using LogAnalyzer;

namespace ModuleLogsProvider.Logging.Most
{
	public sealed class MostNotificationSource : LogNotificationsSourceBase
	{
		private const string DirectoryName = "MOST";

		private readonly List<LogMessageInfo> loadedMessages = new List<LogMessageInfo>();
		private readonly ILogSourceServiceFactory serviceFactory;

		public MostNotificationSource( ITimer timer, ILogSourceServiceFactory serviceFactory )
		{
			if ( timer == null ) throw new ArgumentNullException( "timer" );
			if ( serviceFactory == null ) throw new ArgumentNullException( "serviceFactory" );

			this.serviceFactory = serviceFactory;
			timer.Tick += OnTimerTick;
		}

		private void OnTimerTick( object sender, EventArgs e )
		{
			UpdateLogMessages();
		}

		public List<LogMessageInfo> LoadedMessages
		{
			get { return loadedMessages; }
		}

		public void UpdateLogMessages()
		{
			using ( var clientWrapper = serviceFactory.CreateObject() )
			{
				var client = clientWrapper.Inner;

				int startingIndex = LoadedMessages.Count;

				// todo brinchuk try-catch?
				LogMessageInfo[] newMessages = client.GetLinesStartingWithIndex( startingIndex );
				NotifyOnNewMessages( newMessages );

				LoadedMessages.AddRange( newMessages );
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
