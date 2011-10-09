using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ModuleLogsProvider.Interfaces
{
	public sealed class CpuLoadHelper : IDisposable
	{
		private readonly PerformanceCounter cpuLoadCounter;

		public CpuLoadHelper()
		{
			Process process = Process.GetCurrentProcess();
			string processName = process.ProcessName;
			int processId = process.Id;
			string instanceName = GetPerformanceCounterInstanceNameByPID( processId, processName );
			cpuLoadCounter = GetCpuLoadCounter( instanceName );
		}

		public double GetCpuLoad()
		{
			if ( cpuLoadCounter == null )
				return 0;

			double cpuLoad = cpuLoadCounter.NextValue() / Environment.ProcessorCount;
			return cpuLoad;
		}

		private static PerformanceCounter GetCpuLoadCounter( string instanceName )
		{
			if ( String.IsNullOrEmpty( instanceName ) )
				return null;
			return new PerformanceCounter( "Process", "% Processor Time", instanceName );
		}

		private static string GetPerformanceCounterInstanceNameByPID( int processId, string processName )
		{
			PerformanceCounterCategory category = new PerformanceCounterCategory( "Process" );
			string[] instanceNames = category.GetInstanceNames();
			var filteredInstanceNames = instanceNames.Where( name => name.StartsWith( processName ) );

			foreach ( string instanceName in filteredInstanceNames )
			{
				using ( PerformanceCounter processIdCounter = new PerformanceCounter( "Process", "ID Process", instanceName ) )
				{
					int currentProcessId = (int)processIdCounter.NextValue();
					if ( currentProcessId == processId )
						return instanceName;
				}
			}

			return null;
		}

		public void Dispose()
		{
			if ( cpuLoadCounter != null )
			{
				cpuLoadCounter.Dispose();
			}
		}
	}
}
