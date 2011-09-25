using System.Collections.Generic;

namespace LogAnalyzer.Kernel
{
	public interface ILogFileReader
	{
		IList<LogEntry> ReadToEnd( LogEntry lastAddedEntry );
	}
}