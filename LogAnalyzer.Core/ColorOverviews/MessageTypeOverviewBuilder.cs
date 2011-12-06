using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

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

	internal sealed class DelegateEqualityComparer<T, TSelector> : IEqualityComparer<T>
	{
		private readonly Func<T, TSelector> selector;

		public DelegateEqualityComparer( [NotNull] Func<T, TSelector> selector )
		{
			if ( selector == null ) throw new ArgumentNullException( "selector" );
			this.selector = selector;
		}

		public bool Equals( T x, T y )
		{
			var fieldX = selector( x );
			var fieldY = selector( y );

			bool equals = fieldX.Equals( fieldY );
			return equals;
		}

		public int GetHashCode( T obj )
		{
			var field = selector( obj );
			int hashCode = field.GetHashCode();
			return hashCode;
		}
	}
}