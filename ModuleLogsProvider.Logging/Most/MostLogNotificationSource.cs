using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using Awad.Eticket.ModuleLogsProvider.Types;
using LogAnalyzer;
using LogAnalyzer.Kernel;
using LogAnalyzer.Logging;
using ModuleLogsProvider.Logging.Auxilliary;
using ModuleLogsProvider.Logging.MostLogsServices;

namespace ModuleLogsProvider.Logging.Most
{
	public sealed class MostLogNotificationSource : LogNotificationsSourceBase
	{
		public const string DirectoryName = "MOST";

		private readonly List<LogMessageInfo> loadedMessages = new List<LogMessageInfo>();
		private readonly IServiceFactory<ILogSourceService> serviceFactory;
		private readonly IOperationsQueue operationQueue;
		private readonly IErrorReportingService errorReportingService;
		private readonly MostLogMessagesStorage messagesStorage;
		private readonly MostDirectoryInfo directoryInfo;

		public MostLogNotificationSource( ITimer timer, IServiceFactory<ILogSourceService> serviceFactory, IOperationsQueue operationQueue,
			IErrorReportingService errorReportingService )
		{
			if ( timer == null ) throw new ArgumentNullException( "timer" );
			if ( serviceFactory == null ) throw new ArgumentNullException( "serviceFactory" );
			if ( operationQueue == null ) throw new ArgumentNullException( "operationQueue" );
			if ( errorReportingService == null ) throw new ArgumentNullException( "errorReportingService" );

			this.serviceFactory = serviceFactory;
			this.operationQueue = operationQueue;
			this.errorReportingService = errorReportingService;

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
			using ( var clientWrapper = serviceFactory.Create() )
			{
				var client = clientWrapper.Service;

				int startingIndex = loadedMessages.Count;

				try
				{
					var newMessages = client.GetMessages( startingIndex );
					var appendMessagesResult = messagesStorage.AppendMessages( newMessages );
					NotifyOnNewMessages( newMessages, appendMessagesResult );

					loadedMessages.AddRange( newMessages );
				}
				catch ( Exception exc )
				{
					Logger.Instance.WriteLine( MessageType.Error, String.Format( "Exception while receiving log messages from MOST.Logging service: {0}", exc.ToString() ) );

					bool isExpected = clientWrapper.IsExpectedException( exc );
					if ( !isExpected )
						throw;

					errorReportingService.ReportError( exc, ErrorMessages.NetworkErrorWhileConnectingToLogsService );
				}
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
