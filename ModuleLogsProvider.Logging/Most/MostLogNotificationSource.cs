using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Awad.Eticket.ModuleLogsProvider.Types;
using LogAnalyzer;
using LogAnalyzer.Kernel;
using ModuleLogsProvider.Logging.MostLogsServices;

namespace ModuleLogsProvider.Logging.Most
{
	public sealed class MostLogNotificationSource : LogNotificationsSourceBase
	{
		public const string DirectoryName = "MOST";

		private readonly List<LogMessageInfo> loadedMessages = new List<LogMessageInfo>();
		private readonly ILogSourceServiceFactory serviceFactory;
		private readonly IOperationsQueue operationQueue;
		private readonly MostLogMessagesStorage messagesStorage;
		private readonly MostDirectoryInfo directoryInfo;

		public MostLogNotificationSource( ITimer timer, ILogSourceServiceFactory serviceFactory, IOperationsQueue operationQueue )
		{
			if ( timer == null ) throw new ArgumentNullException( "timer" );
			if ( serviceFactory == null ) throw new ArgumentNullException( "serviceFactory" );
			if ( operationQueue == null ) throw new ArgumentNullException( "operationQueue" );

			this.serviceFactory = serviceFactory;
			this.operationQueue = operationQueue;

			timer.Tick += OnTimerTick;

			directoryInfo = new MostDirectoryInfo( this );
			messagesStorage = new MostLogMessagesStorage( DirectoryInfo );
		}

		internal MostLogMessagesStorage MessagesStorage
		{
			get { return messagesStorage; }
		}

		public MostDirectoryInfo DirectoryInfo
		{
			get { return directoryInfo; }
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
				var appendMessagesResult = messagesStorage.AppendMessages( newMessages );
				NotifyOnNewMessages( newMessages, appendMessagesResult );

				loadedMessages.AddRange( newMessages );
			}
		}

		private void NotifyOnNewMessages( IEnumerable<LogMessageInfo> newMessages, AppendMessagesResult appendMessagesResult )
		{
			var groups = newMessages.GroupBy( m => m.LoggerName ).ToList();

			foreach ( var group in groups )
			{
				string loggerName = group.Key;
				bool fileCreated = appendMessagesResult.CreatedLogFiles.Contains( loggerName );

				WatcherChangeTypes changeTypes = fileCreated ? WatcherChangeTypes.Created : WatcherChangeTypes.Changed;

				string fileName = loggerName;

				var args = new FileSystemEventArgs( changeTypes, DirectoryName, fileName );
				if ( changeTypes == WatcherChangeTypes.Created )
				{
					RaiseCreated( args );
				}
				else
				{
					RaiseChanged( args );
				}
			}
		}
	}
}
