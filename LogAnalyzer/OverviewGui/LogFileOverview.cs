using System.Collections.Generic;
using System.Reactive.Concurrency;
using JetBrains.Annotations;
using LogAnalyzer.ColorOverviews;
using LogAnalyzer.GUI.ViewModels;

namespace LogAnalyzer.GUI.OverviewGui
{
	public sealed class LogFileOverview : GroupingOverview
	{
		public LogFileOverview( [NotNull] IList<LogEntry> entries, [NotNull] LogEntriesListViewModel parent )
			: base( entries, parent, new LogFileOverviewBuilder() )
		{
		}
	}
}