using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel;

namespace LogAnalyzer.Logging
{
	public sealed class Logger
	{
		private Logger() { }

		private readonly Stopwatch _timer = Stopwatch.StartNew();

		private static readonly Logger instance = new Logger();
		public static Logger Instance
		{
			get { return instance; }
		}

		public void WriteLine( MessageType messageType, string message )
		{
			if ( !Accepts( messageType ) )
			{
				return;
			}

			string fullMessage = CreateFullMessage( message, messageType );

			foreach ( var writer in _writers )
			{
				writer.WriteLine( fullMessage, messageType );
			}
		}

		private readonly List<MessageType> _acceptedTypes = new List<MessageType>();

		[DesignerSerializationVisibility( DesignerSerializationVisibility.Content )]
		public List<MessageType> AcceptedTypes { get { return _acceptedTypes; } }

		private readonly List<LogWriter> _writers = new List<LogWriter>();

		[DesignerSerializationVisibility( DesignerSerializationVisibility.Content )]
		public List<LogWriter> Writers
		{
			get { return _writers; }
		}

		public bool Accepts( MessageType messageType )
		{
			return _acceptedTypes.Contains( messageType );
		}

		private string CreateFullMessage( string message, MessageType messageType )
		{
			DateTime now = DateTime.Now;

			string typeString = messageType.ToString()[0].ToString();

			string threadString = Thread.CurrentThread.ManagedThreadId.ToString().PadLeft( 3 );
			string logLine = String.Format( "[{0}] [{1}] {2:dd.MM HH:mm:ss.fff}\t({3} ms)\t{4}", typeString, threadString, now, _timer.ElapsedMilliseconds, message );

			return logLine;
		}
	}
}
