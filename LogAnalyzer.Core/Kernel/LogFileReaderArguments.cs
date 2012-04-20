using System;
using System.Text;
using JetBrains.Annotations;
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

		public LogFileReaderArguments() { }

		public LogFileReaderArguments( [NotNull] LogDirectory logDirectory, [NotNull] LogFile logFile )
		{
			if ( logDirectory == null )
			{
				throw new ArgumentNullException( "logDirectory" );
			}
			if ( logFile == null )
			{
				throw new ArgumentNullException( "logFile" );
			}

			Logger = logDirectory.Config.Logger;
			Encoding = logDirectory.Encoding;
			ParentLogFile = logFile;
			GlobalEntriesFilter = logDirectory.EntriesFilter;
			LineParser = logDirectory.LineParser;
		}

		public void Validate()
		{
			if ( LineParser == null )
				throw new ArgumentException( "LineParser should not be null." );
		}
	}
}
