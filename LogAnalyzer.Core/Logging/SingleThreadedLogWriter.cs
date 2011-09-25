using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;

namespace LogAnalyzer
{
	public abstract class SingleThreadedLogWriter : LogWriter
	{
		protected SingleThreadedLogWriter()
		{
			this.loggerThread = new Thread(ThreadProc);
			loggerThread.IsBackground = true;
			loggerThread.Name = this.GetType().Name + ".Thread";
			loggerThread.Start();
		}

		private readonly Thread loggerThread = null;
		private readonly BlockingCollection<string> operationsQueue = new BlockingCollection<string>(new ConcurrentQueue<string>());

		private void ThreadProc(object state)
		{
			while (true)
			{
				string message = operationsQueue.Take();

				OnNewMessage(message);
			}
		}

		protected abstract void OnNewMessage(string message);

		public sealed override void WriteLine(string message)
		{
			operationsQueue.Add(message);
		}
	}
}
