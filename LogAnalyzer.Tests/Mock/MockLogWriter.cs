using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using JetBrains.Annotations;

namespace LogAnalyzer.Tests.Mock
{
	public sealed class MockLogWriter
	{
		private readonly MockLogRecordsSource notificationSource;
		private readonly MockFileInfo file;
		private readonly Thread loggingThread;


		public MockLogWriter( [NotNull] MockLogRecordsSource notificationSource, [NotNull] MockFileInfo file )
		{
			if ( notificationSource == null ) throw new ArgumentNullException( "notificationSource" );
			if ( file == null ) throw new ArgumentNullException( "file" );

			this.notificationSource = notificationSource;
			this.file = file;

			loggingThread = new Thread( ThreadProc );
			loggingThread.Priority = ThreadPriority.Lowest;
			loggingThread.IsBackground = true;
		}

		private int sleepDuration = 10;
		public int SleepDuration
		{
			get { return sleepDuration; }
			set { sleepDuration = value; }
		}

		public void Start()
		{
			loggingThread.Start();
		}

		private void ThreadProc()
		{
			int threadId = Thread.CurrentThread.ManagedThreadId;
			int count = 0;
			Random rnd = new Random();
			char[] severities = new[] { 'I', 'E', 'W', 'D', 'V' };

			while ( true )
			{
				char messageSeverity = severities[rnd.Next(0, severities.Length)];
				file.WriteLogMessage( messageSeverity, threadId, count.ToString() );
				
				notificationSource.RaiseFileChanged( file.FullName );
				count++;

				Thread.Sleep( SleepDuration );
			}
		}
	}
}
