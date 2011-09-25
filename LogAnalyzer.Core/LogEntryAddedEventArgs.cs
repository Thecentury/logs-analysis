using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer
{
	public sealed class LogEntryAddedEventArgs : EventArgs
	{
		public LogEntryAddedEventArgs(LogEntry addedLogEntry)
		{
			if ( addedLogEntry == null )
				throw new ArgumentNullException( "addedLogEntry" );

			AddedLogEntry = addedLogEntry;
		}

		public LogEntry AddedLogEntry { get; private set; }
	}
}
