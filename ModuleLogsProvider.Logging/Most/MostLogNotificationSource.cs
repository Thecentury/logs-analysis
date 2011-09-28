﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using LogAnalyzer;
using LogAnalyzer.Kernel;
using ModuleLogsProvider.Logging.MostLogsServices;

namespace ModuleLogsProvider.Logging.Most
{
	public sealed class MostLogNotificationSource : LogNotificationsSourceBase
	{
		private const string DirectoryName = "MOST";

		private readonly List<LogMessageInfo> loadedMessages = new List<LogMessageInfo>();
		private readonly ILogSourceServiceFactory serviceFactory;
		private readonly IOperationsQueue operationQueue;
		private readonly MostLogMessagesStorage messagesStorage = new MostLogMessagesStorage();

		public MostLogNotificationSource( ITimer timer, ILogSourceServiceFactory serviceFactory, IOperationsQueue operationQueue )
		{
			if ( timer == null ) throw new ArgumentNullException( "timer" );
			if ( serviceFactory == null ) throw new ArgumentNullException( "serviceFactory" );
			if ( operationQueue == null ) throw new ArgumentNullException( "operationQueue" );

			this.serviceFactory = serviceFactory;
			this.operationQueue = operationQueue;

			timer.Tick += OnTimerTick;
		}

		internal MostLogMessagesStorage MessagesStorage
		{
			get { return messagesStorage; }
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

				string fullFileName = Path.Combine( DirectoryName, loggerName );
				RaiseChanged( new FileSystemEventArgs( changeTypes, DirectoryName, fullFileName ) );
			}
		}
	}
}