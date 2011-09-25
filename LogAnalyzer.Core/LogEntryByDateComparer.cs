using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer
{
	/// <summary>
	/// Сравнивает 2 LogEntry по полю Time.
	/// <remarks>
	/// <para/>
	/// Никогда не возвращает 0 как результат сравнения.
	/// Так сделано потому, что у нас вполне могут быть разные записи лога с совершенно одинаковым временем.
	/// </remarks>
	/// </summary>
	public sealed class LogEntryByDateComparer : IComparer<LogEntry>
	{
		private LogEntryByDateComparer() { }

		private static readonly LogEntryByDateComparer instance = new LogEntryByDateComparer();
		public static LogEntryByDateComparer Instance
		{
			get { return instance; }
		}

		public int Compare( LogEntry x, LogEntry y )
		{
			int comparison = x.Time.CompareTo( y.Time );
			if ( comparison == 0 )
			{
				comparison = x.ParentLogFile.FullPath.CompareTo( y.ParentLogFile.FullPath );
				if ( comparison == 0 )
				{
					comparison = x.LineIndex.CompareTo( y.LineIndex );
					if ( comparison == 0 )
					{
						comparison = x == y ? 0 : -1;
					}
				}
			}
			return comparison;
		}
	}
}
