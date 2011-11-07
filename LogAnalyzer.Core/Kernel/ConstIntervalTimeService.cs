using System;
using LogAnalyzer.Properties;

namespace LogAnalyzer.Kernel
{
	public sealed class ConstIntervalTimeService : ITimeService
	{
		public ConstIntervalTimeService()
		{
			this.sortingDelay = Settings.Default.ConstIntervalTimeServiceDefaultDelay;
		}

		public ConstIntervalTimeService( TimeSpan sortingDelay )
		{
			this.sortingDelay = sortingDelay;
		}

		/// <summary>
		/// Разница по времени, после прошествия которой будет считаться, что более ранние LogEntry не будут добавлены.
		/// </summary>
		private readonly TimeSpan sortingDelay;
		public TimeSpan SortingDelay
		{
			get { return sortingDelay; }
		}

		public bool IsRelativelyOld( DateTime current, DateTime max )
		{
			TimeSpan intervalToMax = max - current;

			return intervalToMax > sortingDelay;
		}
	}
}
