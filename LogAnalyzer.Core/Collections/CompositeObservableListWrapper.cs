using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics;
using LogAnalyzer.Collections;
using LogAnalyzer.Extensions;

namespace LogAnalyzer
{
	/// <summary>
	/// Композитный список, объединяющий 2 списка.
	/// <para/>
	/// Использует только индексаторы.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[DebuggerDisplay( "CompositeObservableListWrapper<{TypeForDebugger}> Count = {first.Count}+{second.Count}" )]
	public sealed class CompositeObservableListWrapper<T> : ThinObservableCollection, IList<T>
	{
		private IList<T> first;
		public IList<T> First
		{
			get { return first; }
			internal set
			{
				Condition.Assert( value != null );
				first = value;
			}
		}

		private readonly IList<T> second;
		public IList<T> Second
		{
			get { return second; }
		}

		private readonly object syncRoot = new object();

		public CompositeObservableListWrapper( IList<T> first, IList<T> second )
		{
			if ( first == null )
				throw new ArgumentNullException( "first" );
			if ( second == null )
				throw new ArgumentNullException( "second" );

			this.first = first;
			this.second = second;
		}

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		private string TypeForDebugger
		{
			get { return typeof( T ).Name; }
		}

		public int IndexOf( T item )
		{
			throw new NotImplementedException();
		}

		public void Insert( int index, T item )
		{
			throw new NotImplementedException();
		}

		public void RemoveAt( int index )
		{
			throw new NotImplementedException();
		}

		public T this[int index]
		{
			get
			{
				int firstCount = first.Count;

				T result;
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
			set
			{
				throw new NotImplementedException();
			}
		}

		public void Add( T item )
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
			get { return first.Count + second.Count; }
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
			lock ( syncRoot )
			{
				T[] secondCopy = new T[second.Count];
				second.CopyTo( secondCopy, 0 );

				return new CompositeArrayEnumerator<T>( first, secondCopy );
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
