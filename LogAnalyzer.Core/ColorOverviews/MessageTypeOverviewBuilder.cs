using System.Collections.Generic;

namespace LogAnalyzer.ColorOverviews
{
	public sealed class MessageTypeOverviewBuilder : OverviewBuilderBase<IEnumerable<LogEntry>, LogEntry>
	{
		protected override LogEntry GetValue( IEnumerable<LogEntry> logEntries )
		{
			HashSet<LogEntry> messageTypesSet = new HashSet<LogEntry>( logEntries, new DelegateEqualityComparer<LogEntry, string>( e => e.Type ) );

			var entry = messageTypesSet.FirstOfExistingTypes( "E", "W", "I", "-", "D", "V" );
			return entry;
		}
	}
}