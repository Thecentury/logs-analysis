using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awad.Eticket.ModuleLogsProvider.Types;
using ModuleLogsProvider.Interfaces;
using Morqua.Logging;
using System.ServiceModel;

// D2DB
namespace Awad.Eticket.ModuleLogsProvider
{
	[ServiceBehavior( InstanceContextMode = InstanceContextMode.Single )]
	internal sealed class MostLogSourceService : ILogSourceService, IDisposable
	{
		public readonly ILogger logger;
		private bool isListening = true;
		private readonly IList<LogMessageInfo> messages = new List<LogMessageInfo>();
		private static readonly object sync = new object();

		public MostLogSourceService( ILogger logger )
		{
			if ( logger == null ) throw new ArgumentNullException( "logger" );

			this.logger = logger;
			Loggers.LineWritten += OnNewLogMessageWritten;
		}

		private void OnNewLogMessageWritten( ILogger messagesLogger, LogLineWrittenEventArgs args )
		{
			if ( !isListening )
				return;

			LogMessageInfo messageInfo = new LogMessageInfo
											{
												LoggerName = messagesLogger.Name,
												Message = args.Message,
												MessageType = FromMessageTypeToSeverity( args.MessageType ),
												IndexInAllMessagesList = 0
											};

			if ( !AcceptMessage( messageInfo ) )
				return;

			lock ( sync )
			{
				int index = messages.Count;
				messageInfo.IndexInAllMessagesList = index;

				messages.Add( messageInfo );
			}
		}

		private static MessageSeverity FromMessageTypeToSeverity( MessageType messageType )
		{
			switch ( messageType )
			{
				case MessageType.Verbose:
					return MessageSeverity.Verbose;
				case MessageType.Debug:
					return MessageSeverity.Debug;
				case MessageType.Info:
					return MessageSeverity.Info;
				case MessageType.Warning:
					return MessageSeverity.Warning;
				case MessageType.Error:
					return MessageSeverity.Error;
				default:
					return MessageSeverity.Verbose;
			}
		}

		private bool AcceptMessage( LogMessageInfo messageInfo )
		{
			bool includeByType = messageInfo.MessageType == MessageSeverity.Error ||
								 messageInfo.MessageType == MessageSeverity.Warning;

			string loggerName = messageInfo.LoggerName;
			bool includeByLogger = loggerName != "UserManager" && loggerName != "eticket-development";

			return includeByType && includeByLogger;
		}

		public void Unsubscribe()
		{
			Loggers.LineWritten -= OnNewLogMessageWritten;
		}

		public void ClearMessagesList()
		{
			logger.WriteLine( MessageType.Info, "MostLogSourceService.Clear()" );
			messages.Clear();
		}

		public void StartListening()
		{
			isListening = true;
			LogIsListening();
		}

		private void LogIsListening()
		{
			logger.WriteLine( MessageType.Info, "MostLogSourceService.IsListening = {0}", isListening );
		}

		public void StopListening()
		{
			isListening = false;
			LogIsListening();
		}

		public LogMessageInfo[] GetMessages( int startingIndex )
		{
			int lastIndex = messages.Count;
			int count = lastIndex - startingIndex;

			LogMessageInfo[] result = new LogMessageInfo[count];

			int arrayIndex = 0;
			for ( int i = startingIndex; i < lastIndex; i++ )
			{
				result[arrayIndex] = messages[i];
				arrayIndex++;
			}

			logger.WriteLine( MessageType.Info,
				String.Format( "Returned {0} messages starting from {1}", result.Length, startingIndex ) );

			return result;
		}

		public int GetMessagesCount()
		{
			return messages.Count;
		}

		public bool GetIsListening()
		{
			return isListening;
		}

		public void Dispose()
		{
			Unsubscribe();
		}
	}
}
