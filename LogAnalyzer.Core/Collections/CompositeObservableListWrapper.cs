using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using JetBrains.Annotations;
using LogAnalyzer.Extensions;

namespace LogAnalyzer.Collections
{
	/// <summary>
	/// Композитный список, объединяющий 2 списка.
	/// <para/>
	/// Использует только индексаторы.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[DebuggerDisplay( "CompositeObservableListWrapper<{TypeForDebugger}> Count = {First.Count}+{Second.Count}" )]
	public sealed class CompositeObservableListWrapper<T> : ThinObservableCollection, IList<T>
	{
		private IList<T> _first;
		public IList<T> First
		{
			get { return _first; }
			internal set
			{
				Condition.Assert( value != null );
				_first = value;
			}
		}

		private readonly IList<T> _second;
		public IList<T> Second
		{
			get { return _second; }
		}

		private readonly object _syncRoot = new object();

		public CompositeObservableListWrapper( IList<T> first, IList<T> second )
		{
			if ( first == null )
				throw new ArgumentNullException( "first" );
			if ( second == null )
				throw new ArgumentNullException( "second" );

			this._first = first;
			this._second = second;
		}

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		[UsedImplicitly]
		private string TypeForDebugger
		{
			get { return typeof( T ).Name; }
		}

		int IList<T>.IndexOf( T item )
		{
			throw new NotImplementedException();
		}

		void IList<T>.Insert( int index, T item )
		{
			throw new NotImplementedException();
		}

		void IList<T>.RemoveAt( int index )
		{
			throw new NotImplementedException();
		}

		public T this[int index]
		{
			get
			{
				int firstCount = _first.Count;

				T result;
				if ( index < firstCount )
				{
					result = _first[index];
				}
				else
				{
					result = _second[index - firstCount];
				}

				return result;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		void ICollection<T>.Add( T item )
		{
			throw new NotImplementedException();
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public bool Contains( T item )
		{
			throw new NotImplementedException();
		}

		public void CopyTo( T[] array, int arrayIndex )
		{
			throw new NotImplementedException();
		}

		public int Count
		{
			get { return _first.Count + _second.Count; }
		}

		public bool IsReadOnly
		{
			get { return true; }
		}

		public bool Remove( T item )
		{
			throw new NotImplementedException();
		}

		public IEnumerator<T> GetEnumerator()
		{
			lock ( _syncRoot )
			{
				T[] secondCopy = new T[_second.Count];
				_second.CopyTo( secondCopy, 0 );

				return new CompositeArrayEnumerator<T>( _first, secondCopy );
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private sealed class CompositeArrayEnumerator<TItem> : IEnumerator<TItem>
		{
			private readonly IList<TItem> first;
			private readonly TItem[] second;
			private readonly int firstCount;
			private readonly int totalLength;

			public CompositeArrayEnumerator( IList<TItem> first, TItem[] second )
			{
				if ( first == null )
					throw new ArgumentNullException( "first" );
				if ( second == null )
					throw new ArgumentNullException( "second" );

				this.first = first;
				this.second = second;
				this.firstCount = first.Count;
				this.totalLength = firstCount + second.Length;
			}

			int index = -1;

			public TItem Current
			{
				get
				{
					TItem result;

					if ( index < firstCount )
					{
						result = first[index];
					}
					else
					{
						result = second[index - firstCount];
					}

					return result;
				}
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

				return index < totalLength;
			}

			public void Reset()
			{
				// do nothing
			}
		}
	}
}
