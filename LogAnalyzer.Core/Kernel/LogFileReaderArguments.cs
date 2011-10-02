using System.Text;
using LogAnalyzer.Filters;
using LogAnalyzer.Logging;

namespace LogAnalyzer.Kernel
{
	public sealed class LogFileReaderArguments
	{
		public Logger Logger { get; set; }
		public Encoding Encoding { get; set; }
		public LogFile ParentLogFile { get; set; }
		public IFilter<LogEntry> GlobalEntriesFilter { get; set; }
	}
}
