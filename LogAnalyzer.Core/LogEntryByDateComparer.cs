using System.Collections.Generic;

namespace LogAnalyzer
{
	public sealed class LogEntryByDateComparer : IComparer<LogEntry>
	{
		public int Compare( LogEntry x, LogEntry y )
		{
			int comparison = x.Time.CompareTo( y.Time );
			if ( comparison == 0 )
			{
				if ( x.ParentLogFile == y.ParentLogFile )
				{
					comparison = x.LineIndex.CompareTo( y.LineIndex );
				}
				else
				{
					comparison = x.GetHashCode().CompareTo(y.GetHashCode());
					if (comparison == 0)
					{

					}
				}
			}
			return comparison;
		}
	}
}