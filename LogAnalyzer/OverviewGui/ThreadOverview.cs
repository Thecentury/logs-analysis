using System.Collections.Generic;
using JetBrains.Annotations;
using LogAnalyzer.ColorOverviews;
using LogAnalyzer.GUI.ViewModels;

namespace LogAnalyzer.GUI.OverviewGui
{
	public sealed class ThreadOverview : GroupingOverview
	{
		public ThreadOverview( [NotNull] IList<LogEntry> entries, [NotNull] LogEntriesListViewModel parent )
			: base( entries, parent, new ThreadOverviewBuilder() )
		{
		}

		protected override string GetIcon()
		{
			return "processor.png";
		}

		public override string Tooltip
		{
			get { return "Thread"; }
		}
	}
}