using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.ComponentModel;

namespace LogAnalyzer
{
	public sealed class Logger
	{
		private static Logger instance = null;
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

		public void WriteLine( string message, MessageType messageType )
		{
			if ( !Accepts( messageType ) )
				return;

			string fullMessage = CreateFullMessage( message, messageType );

			foreach ( var writer in writers )
			{
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

			string logLine = String.Format( "[{0}] {1} {2}", typeString, now.ToString( "HH:mm:ss:fff" ), message );

			return logLine;
		}
	}
}
