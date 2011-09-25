using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Extensions;

namespace ModuleLogsProvider.Logging.Mocks
{
	public sealed class MockTimer : ITimer
	{
		public void MakeRing()
		{
			Tick.Raise( this );
		}

		public event EventHandler Tick;
	}
}
