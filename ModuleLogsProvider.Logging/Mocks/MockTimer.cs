using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Extensions;

namespace ModuleLogsProvider.Logging.Mocks
{
	public sealed class MockTimer : ITimer
	{
		public void Invoke()
		{
			Tick.Raise( this );
		}

		public TimeSpan Interval
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public void Start()
		{
			throw new NotImplementedException();
		}

		public void Stop()
		{
			throw new NotImplementedException();
		}

		public event EventHandler Tick;
	}
}
