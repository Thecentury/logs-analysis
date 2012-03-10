using System.Collections.Generic;
using System.Linq;

namespace LogAnalyzer.ColorOverviews
{
	internal static class LogEntryEnumerableExtensions
	{
		public static LogEntry FirstWithType( this IEnumerable<LogEntry> entries, string type )
		{
			return entries.FirstOrDefault( e => e.Type == type );
		}

		public static LogEntry FirstOfExistingTypes( this ICollection<LogEntry> entries, params  string[] types )
		{
			foreach ( var type in types )
			{
				var entry = entries.FirstWithType( type );
				if ( entry != null )
					return entry;
			}

			return null;
		}
	}
}