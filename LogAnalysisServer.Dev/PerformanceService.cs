using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ModuleLogsProvider.Interfaces;

namespace LogAnalysisServer.Dev
{
	internal sealed class PerformanceService : IPerformanceInfoService
	{
		private readonly Random rnd = new Random();
		public double GetCPULoad()
		{
			return 30 + rnd.NextDouble() * 20;
		}

		public double GetMemoryConsumption()
		{
			return Process.GetCurrentProcess().WorkingSet64;
		}
	}
}
