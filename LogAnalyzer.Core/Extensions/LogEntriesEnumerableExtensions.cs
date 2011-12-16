using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LogAnalyzer.Extensions
{
	public static class LogEntriesEnumerableExtensions
	{
		public static void SaveAll( this IEnumerable<LogEntry> entries, TextWriter writer )
		{
			foreach ( var logEntry in entries )
			{
				logEntry.Write( writer );
			}
		}
	}
}
