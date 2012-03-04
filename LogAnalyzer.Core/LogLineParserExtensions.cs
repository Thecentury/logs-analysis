using System;
using LogAnalyzer.Kernel;

namespace LogAnalyzer
{
	public static class LogLineParserExtensions
	{
		public static LogEntry ParseHeader( this ILogLineParser parser, string line, int lineIndex = 0, LogFile logFile = null )
		{
			string type = null;
			int threadId = 0;
			DateTime time = default( DateTime );
			string text = null;

			if ( !parser.TryExtractLogEntryData( line, ref type, ref threadId, ref time, ref text ) )
			{
				throw new InvalidOperationException( "Failed to parse line." );
			}

			LogEntry entry = new LogEntry( type, threadId, time, text, lineIndex, logFile );
			return entry;
		}
	}
}