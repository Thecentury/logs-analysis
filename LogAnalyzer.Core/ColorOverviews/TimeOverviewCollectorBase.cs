using System;
using System.Collections.Generic;

namespace LogAnalyzer.ColorOverviews
{
	public abstract class TimeOverviewCollectorBase<TItem, TResult> : OverviewCollectorBase<TItem, TResult>
		where TItem : IHaveTime
	{
		public override TResult[] Build( IList<TItem> entries )
		{
			int count = SegmentsCount;
			var result = new TResult[count];

			DateTime minTime = entries[0].Time;
			DateTime maxTime = entries[entries.Count - 1].Time;

			TimeSpan step = TimeSpan.FromTicks( (long)((maxTime - minTime).Ticks / (double)count) );

			for ( int i = 0; i < count; i++ )
			{
				result[i] = InitResult();
			}

			int entriesIndex = 0;
			for ( int i = 0; i < count; i++ )
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