using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace LogAnalyzer
{
	/// <summary>
	/// Итератор, который ходит по IList через индексатор.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed class IndexerEnumerator<T> : IEnumerator<T>
	{
		private readonly IList<T> list;
		private readonly int count;
		private int index = -1;

		public IndexerEnumerator( IList<T> list )
		{
			if ( list == null )
				throw new ArgumentNullException( "list" );

			this.list = list;
			this.count = list.Count;
		}

		public T Current
		{
			get { return list[index]; }
		}

		public void Dispose()
		{
			// do nothing
		}

		object IEnumerator.Current
		{
			get { return Current; }
		}

		public bool MoveNext()
		{
			index++;

			return index < count;
		}

		public void Reset()
		{
			throw new NotSupportedException();
		}
	}

	public sealed class IndexerEnumerable<T> : IEnumerable<T>
	{
		private readonly IList<T> list;

		public IndexerEnumerable( IList<T> list )
		{
			if ( list == null )
				throw new ArgumentNullException( "list" );

			this.list = list;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return new IndexerEnumerator<T>( list );
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
