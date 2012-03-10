using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using LogAnalyzer.ColorOverviews;
using LogAnalyzer.GUI.OverviewGui;

namespace LogAnalyzer.GUI.ViewModels
{
	internal sealed class MessageTypeOverview : OverviewViewModelBase
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

		private void UpdateOverviews()
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

			SetOverviews( list );
		}

		private Task _populationTask;
		private bool _overviewsPopulated;
		private List<OverviewInfo> _overviews = new List<OverviewInfo>();

		public override IEnumerable Items
		{
			get
			{
				if ( !_overviewsPopulated && _populationTask == null )
				{
					_populationTask = new Task( UpdateOverviews );
					_populationTask.ContinueWith( t =>
					{
						_overviewsPopulated = true;
						_populationTask = null;
					} );

					_populationTask.Start();
				}
				return _overviews;
			}
		}

		private void SetOverviews( List<OverviewInfo> overviews )
		{
			_overviews = overviews;
			RaisePropertyChanged( "Items" );
		}
	}
}
