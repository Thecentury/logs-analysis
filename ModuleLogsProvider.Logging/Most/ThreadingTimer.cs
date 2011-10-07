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
		private readonly TimeSpan updateInterval;

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

		public TimeSpan Interval
		{
			get { return updateInterval; }
			set { throw new NotSupportedException(); }
		}
	}
}
