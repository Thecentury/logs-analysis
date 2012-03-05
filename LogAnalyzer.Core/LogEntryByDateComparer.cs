using System.Collections.Generic;

namespace LogAnalyzer
{
	public sealed class LogEntryByDateComparer : IComparer<LogEntry>
	{
		public int Compare( LogEntry x, LogEntry y )
		{
			int comparison = x.Time.CompareTo( y.Time );
			return comparison;
		}
	}
}