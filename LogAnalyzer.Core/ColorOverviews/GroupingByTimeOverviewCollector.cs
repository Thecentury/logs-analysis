using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace LogAnalyzer.ColorOverviews
{
	public sealed class GroupingByTimeOverviewCollector<T> : TimeOverviewCollectorBase<T, IList<T>>
		where T : IHaveTime
	{
		private readonly Func<T, bool> _filter;

		public GroupingByTimeOverviewCollector()
			: this( e => true )
		{
		}

		public GroupingByTimeOverviewCollector( [NotNull] Func<T, bool> filter )
		{
			if ( filter == null ) throw new ArgumentNullException( "filter" );
			this._filter = filter;
		}

		protected override IList<T> InitResult()
		{
			return new List<T>();
		}

		protected override void Append( TimeProxy<IList<T>>[] result, int index, T entry )
		{
			if ( _filter( entry ) )
			{
				var list = result[index].Item;
				list.Add( entry );
			}
		}
	}

	public sealed class GroupingByIndexOverviewCollector<T> : IndexOverviewCollectorBase<T, IList<T>>
	{
		private readonly Func<T, bool> _filter;

		public GroupingByIndexOverviewCollector()
			: this( e => true )
		{
		}

		public GroupingByIndexOverviewCollector( [NotNull] Func<T, bool> filter )
		{
			if ( filter == null ) throw new ArgumentNullException( "filter" );
			this._filter = filter;
		}

		protected override IList<T> InitResult()
		{
			return new List<T>();
		}

		protected override void Append( IList<T>[] result, int index, T entry )
		{
			if ( _filter( entry ) )
			{
				var list = result[index];
				list.Add( entry );
			}
		}
	}
}
