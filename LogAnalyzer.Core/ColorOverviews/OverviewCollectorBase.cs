using System;
using System.Collections.Generic;

namespace LogAnalyzer.ColorOverviews
{
	public abstract class OverviewCollectorBase<TItem, TResult> : IOverviewCollector<TItem, TResult>
		where TItem : IHaveTime
	{
		private int segmentsCount = 1000;

		public int SegmentsCount
		{
			get { return segmentsCount; }
			set { segmentsCount = value; }
		}

		protected abstract TResult InitResult();
		protected abstract void Append( TResult[] result, int index, TItem entry );

		public TResult[] Build( IList<TItem> entries )
		{
			var result = new TResult[segmentsCount];

			DateTime minTime = entries[0].Time;
			DateTime maxTime = entries[entries.Count - 1].Time;

			TimeSpan step = TimeSpan.FromTicks( (long)((maxTime - minTime).Ticks / (double)segmentsCount) );

			for ( int i = 0; i < segmentsCount; i++ )
			{
				result[i] = InitResult();
			}

			int entriesIndex = 0;
			for ( int i = 0; i < segmentsCount; i++ )
			{
				DateTime stepUpperBoundary = minTime + TimeSpan.FromSeconds( step.TotalSeconds * i );

				bool finished = false;

				do
				{
					var entry = entries[entriesIndex];
					if ( entry.Time <= stepUpperBoundary )
					{
						Append( result, i, entry );
						entriesIndex++;
						if ( entriesIndex >= entries.Count )
						{
							finished = true;
							break;
						}
					}
					else
					{
						break;
					}
				} while ( true );

				if ( finished )
				{
					break;
				}
			}

			return result;
		}
	}
}