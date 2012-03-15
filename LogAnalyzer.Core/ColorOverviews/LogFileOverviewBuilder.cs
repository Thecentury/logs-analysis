using System.Collections.Generic;
using System.Linq;

namespace LogAnalyzer.ColorOverviews
{
	public sealed class LogFileOverviewBuilder : OverviewBuilderBase<IEnumerable<LogEntry>, LogEntry>
	{
		protected override LogEntry GetValue( IEnumerable<LogEntry> logEntries )
		{
			LogEntry mostFrequentFileEntry = (from entry in logEntries
											  group entry by entry.ParentLogFile.Name
												  into g
												  let head = new { FileName = g.Key, First = g.First(), Count = g.Count() }
												  orderby head.Count descending
												  select head.First).FirstOrDefault();

			return mostFrequentFileEntry;
		}
	}
}