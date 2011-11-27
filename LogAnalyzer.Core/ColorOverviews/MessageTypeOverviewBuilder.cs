using System.Collections.Generic;
using System.Linq;

namespace LogAnalyzer.ColorOverviews
{
	public sealed class MessageTypeOverviewBuilder : OverviewBuilderBase<IEnumerable<LogEntry>, string>
	{
		protected override string GetValue( IEnumerable<LogEntry> item )
		{
			HashSet<string> messageTypesSet = new HashSet<string>( item.Select( le => le.Type ) );

			if ( messageTypesSet.Contains( "E" ) )
				return "E";
			if ( messageTypesSet.Contains( "W" ) )
				return "W";
			if ( messageTypesSet.Contains( "I" ) )
				return "I";
			if ( messageTypesSet.Contains( "D" ) )
				return "D";
			if ( messageTypesSet.Contains( "V" ) )
				return "V";
			return "-";
		}
	}
}