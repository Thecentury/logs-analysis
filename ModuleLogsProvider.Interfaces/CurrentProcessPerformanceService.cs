using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace ModuleLogsProvider.Interfaces
{
	[ServiceBehavior( InstanceContextMode = InstanceContextMode.Single )]
	public sealed class CurrentProcessPerformanceService : IPerformanceInfoService
	{
		private readonly CpuLoadHelper cpuLoadHelper = new CpuLoadHelper();
		private readonly Process currentProcess = Process.GetCurrentProcess();

		public double GetCPULoad()
		{
			return cpuLoadHelper.GetCpuLoad();
		}

		public double GetMemoryConsumption()
		{
			return currentProcess.WorkingSet64;
		}
	}
}
