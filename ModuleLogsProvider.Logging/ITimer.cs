using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModuleLogsProvider.Logging
{
	public interface ITimer
	{
		event EventHandler Tick;
		void Invoke();

		TimeSpan Interval { get; set; }
	}
}
