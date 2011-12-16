using System;
using System.Collections.Generic;
using System.Threading;
using System.ComponentModel;

namespace LogAnalyzer.Logging
{
	public sealed class Logger
	{
		private static volatile Logger instance;
		public static Logger Instance
		{
			get { return Logger.instance; }
		}

		public Logger()
		{
			if ( instance == null )
			{
				lock ( typeof( Logger ) )
				{
					if ( instance == null )
					{
						instance = this;
					}
				}
			}
		}

		public void WriteLine( MessageType messageType, string message )
		{
			if ( !Accepts( messageType ) )
				return;

			string fullMessage = CreateFullMessage( message, messageType );

			for ( int i = 0; i < writers.Count; i++ )
			{
				var writer = writers[i];
				writer.WriteLine( fullMessage );
			}
		}

		private readonly List<MessageType> acceptedTypes = new List<MessageType>();

		[DesignerSerializationVisibility( DesignerSerializationVisibility.Content )]
		public List<MessageType> AcceptedTypes { get { return acceptedTypes; } }

		private readonly List<LogWriter> writers = new List<LogWriter>();

		[DesignerSerializationVisibility( DesignerSerializationVisibility.Content )]
		public List<LogWriter> Writers
		{
			get { return writers; }
		}

		public bool Accepts( MessageType messageType )
		{
			return acceptedTypes.Contains( messageType );
		}

		private static string CreateFullMessage( string message, MessageType messageType )
		{
			DateTime now = DateTime.Now;

			string typeString = messageType.ToString()[0].ToString();

			string threadString = Thread.CurrentThread.ManagedThreadId.ToString().PadLeft( 3 );
			string logLine = String.Format( "[{0}] [{1}] {2:G} {3}", typeString, threadString, now, message );

			return logLine;
		}
	}
}
