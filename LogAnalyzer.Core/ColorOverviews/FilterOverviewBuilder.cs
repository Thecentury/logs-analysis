using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LogAnalyzer.Filters;

namespace LogAnalyzer.ColorOverviews
{
	public sealed class FilterOverviewBuilder : OverviewBuilderBase<IEnumerable<LogEntry>, LogEntry>
	{
		private readonly IFilter<LogEntry> _filter;

		public FilterOverviewBuilder( [NotNull] IFilter<LogEntry> filter )
		{
			if ( filter == null )
			{
				throw new ArgumentNullException( "filter" );
			}
			_filter = filter;
		}

		protected override LogEntry GetValue( IEnumerable<LogEntry> logEntries )
		{
			LogEntry filtered = logEntries.FirstOrDefault( _filter.Include );
			return filtered;
		}
	}
}