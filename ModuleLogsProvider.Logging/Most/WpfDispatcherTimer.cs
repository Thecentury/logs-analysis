using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using LogAnalyzer.Extensions;

namespace ModuleLogsProvider.Logging.Most
{
	public sealed class WpfDispatcherTimer : ITimer
	{
		private readonly DispatcherTimer timer;
		public WpfDispatcherTimer()
		{
			timer = new DispatcherTimer( DispatcherPriority.Normal, DispatcherHelper.GetDispatcher() );
			timer.Tick += OnTimerTick;
			timer.Start();
		}

		public WpfDispatcherTimer( TimeSpan updateInterval )
			: this()
		{
			Interval = updateInterval;
		}

		private void OnTimerTick( object sender, EventArgs e )
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
			get { return timer.Interval; }
			set { timer.Interval = value; }
		}

		public void Start()
		{
			timer.Start();
		}

		public void Stop()
		{
			timer.Stop();
		}
	}
}
