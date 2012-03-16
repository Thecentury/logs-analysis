using System;
using System.Collections.Generic;

namespace LogAnalyzer.ColorOverviews
{
	public struct TimeProxy<T>
	{
		private DateTime _time;
		public DateTime Time
		{
			get { return _time; }
			set { _time = value; }
		}

		private T _item;
		public T Item
		{
			get { return _item; }
			set { _item = value; }
		}

		public TimeProxy( DateTime time, T item )
		{
			_time = time;
			_item = item;
		}
	}

	public abstract class TimeOverviewCollectorBase<TItem, TResult> : OverviewCollectorBase<TItem, TimeProxy<TResult>>
		where TItem : IHaveTime
	{
		protected abstract TResult InitResult();

		public override TimeProxy<TResult>[] Build( IList<TItem> entries )
		{
			if ( entries.Count == 0 )
			{
				return new TimeProxy<TResult>[0];
			}

			int count = SegmentsCount;
			var result = new TimeProxy<TResult>[count];

			DateTime minTime = entries[0].Time;
			DateTime maxTime = entries[entries.Count - 1].Time;

			TimeSpan step = TimeSpan.FromTicks( (long)((maxTime - minTime).Ticks / (double)count) );

			int entriesIndex = 0;

			for ( int i = 0; i < count; i++ )
			{
				DateTime stepUpperBoundary = minTime + TimeSpan.FromSeconds( step.TotalSeconds * i );
				result[i] = new TimeProxy<TResult>( stepUpperBoundary, InitResult() );

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