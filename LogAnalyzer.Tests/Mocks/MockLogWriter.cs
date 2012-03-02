using System;
using System.Threading;
using JetBrains.Annotations;

namespace LogAnalyzer.Tests.Mocks
{
	public sealed class MockLogWriter
	{
		private readonly MockLogRecordsSource _notificationSource;
		private readonly MockFileInfo _file;
		private readonly Thread _loggingThread;

		public MockLogWriter( [NotNull] MockLogRecordsSource notificationSource, [NotNull] MockFileInfo file, TimeSpan sleepDuration )
		{
			if ( notificationSource == null ) throw new ArgumentNullException( "notificationSource" );
			if ( file == null ) throw new ArgumentNullException( "file" );

			this._notificationSource = notificationSource;
			this._file = file;
			this._sleepDuration = sleepDuration;

			_loggingThread = new Thread( ThreadProc );
			_loggingThread.Priority = ThreadPriority.Lowest;
			_loggingThread.IsBackground = true;
		}

		private TimeSpan _sleepDuration = TimeSpan.FromMilliseconds( 10 );
		public TimeSpan SleepDuration
		{
			get { return _sleepDuration; }
			set { _sleepDuration = value; }
		}

		public void Start()
		{
			_loggingThread.Start();
		}

		private void ThreadProc()
		{
			int threadId = Thread.CurrentThread.ManagedThreadId;
			int count = 0;
			Random rnd = new Random();
			char[] severities = new[] { 'I', 'E', 'W', 'D', 'V' };

			while ( true )
			{
				char messageSeverity = severities[rnd.Next( 0, severities.Length )];
				_file.WriteLogMessage( messageSeverity, threadId, count.ToString() );

				_notificationSource.RaiseFileChanged( _file.FullName );
				count++;

				Thread.Sleep( _sleepDuration );
			}
		}
	}
}
