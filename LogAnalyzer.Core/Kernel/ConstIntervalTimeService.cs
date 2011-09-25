using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer
{
	public sealed class ConstIntervalTimeService : ITimeService
	{
		public ConstIntervalTimeService() { }
		public ConstIntervalTimeService( TimeSpan sortingDelay )
		{
			this.sortingDelay = sortingDelay;
		}

		/// <summary>
		/// Разница по времени, после прошествия которой будет считаться, что более ранние LogEntry не будут добавлены.
		/// </summary>
		private readonly TimeSpan sortingDelay = TimeSpan.FromSeconds( 20 );
		public TimeSpan SortingDelay
		{
			get { return sortingDelay; }
		}

		#region ITimeService Members

		public bool IsRelativelyOld( DateTime current, DateTime max )
		{
			TimeSpan intervalToNow = DateTime.Now - current;
			TimeSpan intervalToMax = max - current;
			TimeSpan maxInterval = intervalToNow > intervalToMax ? intervalToNow : intervalToMax;

			return maxInterval > sortingDelay;
		}

		#endregion
	}
}
