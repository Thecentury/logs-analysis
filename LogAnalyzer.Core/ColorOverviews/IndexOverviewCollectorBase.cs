using System;
using System.Collections.Generic;

namespace LogAnalyzer.ColorOverviews
{
	public abstract class IndexOverviewCollectorBase<TItem, TResult> : OverviewCollectorBase<TItem, TResult>
	{
		private int _maxSegmentsCount = 1000;
		public int MaxSegmentsCount
		{
			get { return _maxSegmentsCount; }
			set { _maxSegmentsCount = value; }
		}

		public sealed override TResult[] Build( IList<TItem> items )
		{
			int count = Math.Min( _maxSegmentsCount, items.Count );
			TResult[] result = new TResult[count];

			for ( int i = 0; i < count; i++ )
			{
				result[i] = InitResult();
			}

			double delta = items.Count / ((double)count);
			int itemsIndex = 0;
			for ( int i = 0; i < count; i++ )
			{
				double currentCeiling = delta * i;
				while ( itemsIndex < currentCeiling )
				{
					var entry = items[itemsIndex];
					Append( result, i, entry );

					itemsIndex++;
				}
			}

			return result;
		}
	}
}