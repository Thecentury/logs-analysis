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
		private readonly IList<T> _list;
		private readonly int _count;
		private int _index = -1;

		public IndexerEnumerator( IList<T> list )
		{
			if ( list == null )
			{
				throw new ArgumentNullException( "list" );
			}

			this._list = list;
			this._count = list.Count;
		}

		public T Current
		{
			get { return _list[_index]; }
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
			_index++;

			return _index < _count;
		}

		public void Reset()
		{
			throw new NotSupportedException();
		}
	}

	public sealed class IndexerEnumerable<T> : IEnumerable<T>
	{
		private readonly IList<T> _list;

		public IndexerEnumerable( IList<T> list )
		{
			if ( list == null )
				throw new ArgumentNullException( "list" );

			this._list = list;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return new IndexerEnumerator<T>( _list );
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
