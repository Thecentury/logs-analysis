﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using LogAnalyzer.Extensions;

namespace ModuleLogsProvider.Logging.Most
{
	public sealed class ThreadingTimer : ITimer
	{
		// ReSharper disable NotAccessedField.Local
		private readonly Timer logsPollTimer;
		// ReSharper restore NotAccessedField.Local

		public ThreadingTimer( TimeSpan logsUpdateInterval )
		{
			int milliseconds = (int)logsUpdateInterval.TotalMilliseconds;

			logsPollTimer = new Timer( OnTimerTick, null, milliseconds, milliseconds );
		}

		private void OnTimerTick( object state )
		{
			Tick.Raise( this );
		}

		public event EventHandler Tick;

		public void MakeRing()
		{
			Tick.Raise( this );
		}
	}
}
