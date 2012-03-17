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

		protected override string GetIcon()
		{
			return "document-text.png";
		}

		public override string Tooltip
		{
			get { return "LogFile"; }
		}
	}
}