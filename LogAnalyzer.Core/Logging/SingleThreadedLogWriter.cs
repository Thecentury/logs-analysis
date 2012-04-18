using System.Threading;
using System.Collections.Concurrent;

namespace LogAnalyzer.Logging
{
	public abstract class SingleThreadedLogWriter : LogWriter
	{
		protected SingleThreadedLogWriter()
		{
			this._loggerThread = new Thread( ThreadProc );
			_loggerThread.IsBackground = true;
			_loggerThread.Name = this.GetType().Name + ".Thread";
			_loggerThread.Start();
		}

		private readonly Thread _loggerThread;
		private readonly BlockingCollection<LogMessage> _operationsQueue = new BlockingCollection<LogMessage>( new ConcurrentQueue<LogMessage>() );

		private void ThreadProc( object state )
		{
			while ( true )
			{
				var message = _operationsQueue.Take();

				OnNewMessage( message );
			}
		}

		protected abstract void OnNewMessage( LogMessage message );

		public sealed override void WriteLine( string message, MessageType messageType )
		{
			_operationsQueue.Add( new LogMessage { Message = message, MessageType = messageType } );
		}

		protected sealed class LogMessage
		{
			public string Message { get; set; }
			public MessageType MessageType { get; set; }
		}
	}
}
