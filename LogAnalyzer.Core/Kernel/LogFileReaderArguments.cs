using System;
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
		public ILogLineParser LineParser { get; set; }

		public void Validate()
		{
			if ( LineParser == null )
				throw new ArgumentException( "LineParser should not be null." );
		}
	}
}
