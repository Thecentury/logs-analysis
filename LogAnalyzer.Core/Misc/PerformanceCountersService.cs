using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using LogAnalyzer.Properties;

namespace LogAnalyzer
{
	public static class PerformanceCountersService
	{
		public static readonly string Category = "AWAD.LogAnalyzer";
		public static readonly string PendingOperationsCount = "# of pending operations";

		public static void Increment( PerformanceCounter counter )
		{
			if ( counter == null )
				return;

			counter.Increment();
		}

		public static void Decrement( PerformanceCounter counter )
		{
			if ( counter == null )
				return;

			counter.Decrement();
		}

		public static PerformanceCounter GetCounter( string categoryName, string counterName )
		{
			if ( !Settings.Default.PerfCountersEnabled )
				return null;

			if ( !PerformanceCounterCategory.Exists( Category ) )
			{
				PerformanceCounterCategory.Create( Category, "AWAD.LogAnalyzer counters",
					PerformanceCounterCategoryType.SingleInstance,
					new CounterCreationDataCollection
					{
						new CounterCreationData(PendingOperationsCount, PendingOperationsCount, PerformanceCounterType.NumberOfItems32)
					}
					);
			}

			return new PerformanceCounter( categoryName, counterName, false );
		}

		public static PerformanceCounter GetPendingOperationsCountCounter()
		{
			return GetCounter( Category, PendingOperationsCount );
		}
	}
}
