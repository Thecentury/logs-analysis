using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Media;
using JetBrains.Annotations;
using LogAnalyzer.ColorOverviews;
using LogAnalyzer.Filters;
using LogAnalyzer.GUI.ViewModels;

namespace LogAnalyzer.GUI.OverviewGui
{
	public sealed class FilterOverview : AsyncOverview
	{
		private readonly IList<LogEntry> _entries;
		private readonly LogEntriesListViewModel _parent;
		private readonly GroupingByIndexOverviewCollector<LogEntry> _collector;
		private FilterOverviewBuilder _builder;

		public FilterOverview( [NotNull] IList<LogEntry> entries, [NotNull] LogEntriesListViewModel parent )
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
		}

		private IFilter<LogEntry> _filter;
		public IFilter<LogEntry> Filter
		{
			get { return _filter; }
			set
			{
				_filter = value;
				Update();
			}
		}

		private Brush _brush;
		public Brush Brush
		{
			get { return _brush; }
			set
			{
				_brush = value;
				RaisePropertyChanged( "Brush" );
			}
		}

		protected override IEnumerable UpdateOverviews()
		{
			if ( Filter == null )
			{
				return new object[0];
			}

			_builder = new FilterOverviewBuilder( Filter );

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

				OverviewInfo info = new OverviewInfo( map[i], i / length, _parent );
				list.Add( info );
			}

			return list;
		}

		public override string Tooltip
		{
			get { return "Filter"; }
		}

		protected override string GetIcon()
		{
			return "funnel.png";
		}
	}
}