using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using JetBrains.Annotations;
using LogAnalyzer.ColorOverviews;
using LogAnalyzer.Extensions;
using LogAnalyzer.GUI.ViewModels;

namespace LogAnalyzer.GUI.OverviewGui
{
	public abstract class GroupingOverview : AsyncOverview
	{
		private readonly IList<LogEntry> _entries;
		private readonly LogEntriesListViewModel _parent;
		private readonly GroupingByIndexOverviewCollector<LogEntry> _collector;
		private readonly OverviewBuilderBase<IEnumerable<LogEntry>, LogEntry> _overviewBuilder;

		protected GroupingOverview( [NotNull] IList<LogEntry> entries, [NotNull] LogEntriesListViewModel parent, [NotNull] OverviewBuilderBase<IEnumerable<LogEntry>, LogEntry> overviewBuilder )
		{
			if ( entries == null )
			{
				throw new ArgumentNullException( "entries" );
			}
			if ( parent == null )
			{
				throw new ArgumentNullException( "parent" );
			}
			if ( overviewBuilder == null )
			{
				throw new ArgumentNullException( "overviewBuilder" );
			}

			_entries = entries;
			_parent = parent;
			_collector = new GroupingByIndexOverviewCollector<LogEntry>();
			_overviewBuilder = overviewBuilder;

			INotifyCollectionChanged observableCollection = _entries as INotifyCollectionChanged;
			if ( observableCollection != null )
			{
				var observable = Observable.FromEventPattern<NotifyCollectionChangedEventArgs>( observableCollection,
																							   "CollectionChanged" );
				var scheduler = parent.ApplicationViewModel.Config.ResolveNotNull<IScheduler>();

				observable
					.Buffer( TimeSpan.FromSeconds( 1 ) )
					.Where( e => e.Count > 0 )
					.ObserveOn( scheduler )
					.Subscribe( e => Update() );
			}
		}

		protected sealed override IEnumerable UpdateOverviews()
		{
			var map = _overviewBuilder.CreateOverviewMap( _collector.Build( _entries ) );

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

	}
}