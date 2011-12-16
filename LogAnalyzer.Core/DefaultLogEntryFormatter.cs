using System;

namespace LogAnalyzer
{
	public sealed class DefaultLogEntryFormatter : ILogEntryFormatter
	{
		public string Format( LogEntry logEntry )
		{
			string text = String.Format( "[{0}] [{1}] {2:G}\t{3}{4}", logEntry.Type, logEntry.ThreadId.ToString().PadLeft( 3 ), logEntry.Time, logEntry.UnitedText,
			                             Environment.NewLine );
			return text;
		}
	}
}