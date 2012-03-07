using System.Collections.Generic;

namespace LogAnalyzer
{
	public sealed class LogEntryByTimeComparer : IComparer<LogEntry>
	{
		private static readonly LogEntryByTimeComparer instance = new LogEntryByTimeComparer();
		public static LogEntryByTimeComparer Instance
		{
			get { return instance; }
		}

		public int Compare( LogEntry x, LogEntry y )
		{
			int xSeconds = (int)x.Time.TimeOfDay.TotalSeconds;
			int ySeconds = (int)y.Time.TimeOfDay.TotalSeconds;
			int comparison = xSeconds.CompareTo( ySeconds );
			return comparison;
		}
	}
}