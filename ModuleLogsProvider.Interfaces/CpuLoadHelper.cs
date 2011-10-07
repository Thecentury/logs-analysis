using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ModuleLogsProvider.Interfaces
{
	public sealed class CpuLoadHelper
	{
		private PerformanceCounter cpuLoadCounter;

		public CpuLoadHelper()
		{
			// Process
			// % Processor Time
			// MOSTServer#1
			cpuLoadCounter = new PerformanceCounter( "Процесс", "% загруженности процессора", "" );
		}
	}
}
