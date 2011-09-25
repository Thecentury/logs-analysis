using System;
using System.Collections.Generic;
using System.Threading;
using Awad.Eticket.ModuleLogsProvider.Types;
using LogAnalyzer;

namespace ModuleLogsProvider.Logging.Most
{
	public sealed class MostNotificationSource : LogNotificationsSourceBase
	{
		// ReSharper disable NotAccessedField.Local
		private readonly Timer logsPollTimer;
		// ReSharper restore NotAccessedField.Local

		private readonly List<LogMessageInfo> loadedMessages = new List<LogMessageInfo>();

		private readonly ILogSourceServiceFactory serviceFactory;

		public MostNotificationSource( TimeSpan logsUpdateInterval, ILogSourceServiceFactory serviceFactory )
		{
			if ( serviceFactory == null )
				throw new ArgumentNullException( "serviceFactory" );

			this.serviceFactory = serviceFactory;

			int milliseconds = (int)logsUpdateInterval.TotalMilliseconds;

			logsPollTimer = new Timer( OnTimerTick, null, milliseconds, milliseconds );
		}

		private void OnTimerTick( object state )
		{
			UpdateLogMessages();
		}

		public void UpdateLogMessages()
		{
			using ( var clientWrapper = serviceFactory.CreateObject() )
			{
				var client = clientWrapper.Inner;

				int startingIndex = loadedMessages.Count;

				// todo brinchuk try-catch?
				LogMessageInfo[] newMessages = client.GetLinesStartingWithIndex( startingIndex );

				loadedMessages.AddRange( newMessages );
			}
		}
	}
}
