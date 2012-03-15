using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using JetBrains.Annotations;
using LogAnalyzer.ColorOverviews;
using LogAnalyzer.GUI.ViewModels;

namespace LogAnalyzer.GUI.OverviewGui
{
	public sealed class MessageTypeOverview : GroupingOverview
	{
		public MessageTypeOverview( [NotNull] IList<LogEntry> entries, [NotNull] LogEntriesListViewModel parent )
			: base( entries, parent, new MessageTypeOverviewBuilder() )
		{
		}
	}
}
