using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using LogAnalyzer.ColorOverviews;
using LogAnalyzer.GUI.ViewModels;

namespace LogAnalyzer.GUI.OverviewGui
{
	internal sealed class MessageTypeOverview : AsyncOverview
	{
		private readonly IList<LogEntry> _entries;
		private readonly LogEntriesListViewModel _parent;
		private readonly GroupingByIndexOverviewCollector<LogEntry> _collector;
		private readonly MessageTypeOverviewBuilder _builder;

		public MessageTypeOverview( [NotNull] IList<LogEntry> entries, [NotNull] LogEntriesListViewModel parent )
		{
			if ( entries == null )
			{
				throw new ArgumentNullException( "entries" );
			}
			if ( parent == null )
			{
				throw new ArgumentNullException( "parent" );
			}

			_entries = entries;
			_parent = parent;
			_collector = new GroupingByIndexOverviewCollector<LogEntry>();
			_builder = new MessageTypeOverviewBuilder();
		}

		protected override IEnumerable UpdateOverviews()
		{
			var map = _builder.CreateOverviewMap( _collector.Build( _entries ) );

			double length = map.Length;

			List<OverviewInfo> list = new List<OverviewInfo>();
			for ( int i = 0; i < map.Length; i++ )
			{
				LogEntry entry = map[i];
				if ( entry == null )
				{
					continue;
				}
				//if ( !entry.Type.In( MessageTypes.Error, MessageTypes.Warning ) )
				//    continue;

				OverviewInfo info = new OverviewInfo( map[i], i / length, _parent );
				list.Add( info );
			}

			return list;
		}
	}
}
