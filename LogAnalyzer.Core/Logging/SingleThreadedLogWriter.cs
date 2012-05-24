using System;
using System.Threading;
using System.Collections.Concurrent;

namespace LogAnalyzer.Logging
{
	public abstract class SingleThreadedLogWriter : LogWriter, IDisposable
	{
		protected SingleThreadedLogWriter()
		{
			this._loggerThread = new Thread( ThreadProc );
			_loggerThread.IsBackground = true;
			_loggerThread.Name = this.GetType().Name + ".Thread";
			_loggerThread.Start();
		}

		private readonly Thread _loggerThread;
		private readonly BlockingCollection<ThreadMessage> _operationsQueue = new BlockingCollection<ThreadMessage>( new ConcurrentQueue<ThreadMessage>() );

		private void ThreadProc( object state )
		{
			while ( true )
			{
				var threadMessage = _operationsQueue.Take();
				if ( !threadMessage.AbortRequested )
				{
					OnNewMessage( threadMessage.Message );
				}
				else
				{
					break;
				}
			}
		}

		protected abstract void OnNewMessage( LogMessage message );

		public sealed override void WriteLine( string message, MessageType messageType )
		{
			_operationsQueue.Add( new ThreadMessage( new LogMessage { Message = message, MessageType = messageType } ) );
		}

		protected sealed class LogMessage
		{
			public string Message { get; set; }
			public MessageType MessageType { get; set; }
		}

		public void Dispose()
		{
			_operationsQueue.Add( new ThreadMessage( abortRequested: true ) );
		}

		private sealed class ThreadMessage
		{
			public bool AbortRequested { get; set; }
			public LogMessage Message { get; set; }

			public ThreadMessage( LogMessage message )
			{
				Message = message;
			}

			public ThreadMessage( bool abortRequested )
			{
				AbortRequested = abortRequested;
			}
		}
	}
}
