using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using LogAnalyzer.Extensions;

namespace ModuleLogsProvider.Logging.Most
{
	public sealed class ThreadingTimer : ITimer
	{
		private readonly Timer logsPollTimer;

		public ThreadingTimer( TimeSpan updateInterval )
		{
			this.updateInterval = updateInterval;

			int milliseconds = (int)updateInterval.TotalMilliseconds;

			logsPollTimer = new Timer( OnTimerTick, null, milliseconds, milliseconds );
		}

		private void OnTimerTick( object state )
		{
			Tick.Raise( this );
		}

		public event EventHandler Tick;

		public void Invoke()
		{
			Tick.Raise( this );
		}

		private TimeSpan updateInterval;
		public TimeSpan Interval
		{
			get { return updateInterval; }
			set
			{
				updateInterval = value;
				logsPollTimer.Change( updateInterval, updateInterval );
			}
		}

		public void Start()
		{
			logsPollTimer.Change( TimeSpan.Zero, Interval );
		}

		public void Stop()
		{
			logsPollTimer.Change( Timeout.Infinite, 0 );
		}
	}
}
