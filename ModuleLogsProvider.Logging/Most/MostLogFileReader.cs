using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer;
using LogAnalyzer.Kernel;

namespace ModuleLogsProvider.Logging.Most
{
	internal sealed class MostLogFileReader : LogFileReaderBase
	{
		private readonly ICollection<LogEntry> logEntries;

		public MostLogFileReader( ICollection<LogEntry> logEntries )
		{
			if ( logEntries == null ) throw new ArgumentNullException( "logEntries" );

			this.logEntries = logEntries;
		}

		public override IList<LogEntry> ReadToEnd( LogEntry lastAddedEntry )
		{
			throw new NotImplementedException();
		}
	}
}
