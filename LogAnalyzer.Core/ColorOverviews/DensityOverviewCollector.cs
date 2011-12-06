using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace LogAnalyzer.ColorOverviews
{
	public sealed class DensityOverviewCollector<T> : TimeOverviewCollectorBase<T, ICollection<T>>
		where T : IHaveTime
	{
		private readonly Func<T, bool> predicate;

		public DensityOverviewCollector( [NotNull] Func<T, bool> predicate )
		{
			if ( predicate == null ) throw new ArgumentNullException( "predicate" );
			this.predicate = predicate;
		}

		protected override ICollection<T> InitResult()
		{
			return new CountOnlyCollection<T>();
		}

		protected override void Append( ICollection<T>[] result, int index, T entry )
		{
			if ( predicate( entry ) )
			{
				var collection = result[index];
				collection.Add(entry);
			}
		}

		private sealed class CountOnlyCollection<TItem> : ICollection<TItem>
		{
			private int count;
			public int Count
			{
				get { return count; }
			}

			public bool IsReadOnly
			{
				get { return true; }
			}

			public void Add( TItem item )
			{
				count++;
			}

			#region Not supported

			public IEnumerator<TItem> GetEnumerator()
			{
				throw new NotSupportedException();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			public void Clear()
			{
				throw new NotSupportedException();
			}

			public bool Contains( TItem item )
			{
				throw new NotSupportedException();
			}

			public void CopyTo( TItem[] array, int arrayIndex )
			{
				throw new NotSupportedException();
			}

			public bool Remove( TItem item )
			{
				throw new NotSupportedException();
			}

			#endregion
		}
	}
}