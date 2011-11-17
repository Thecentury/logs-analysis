using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using System.Diagnostics;

namespace LogAnalyzer.Collections
{
	/// <summary>
	/// Добавляет в IList поддержку INotifyCollectionChanged.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[DebuggerDisplay( "Count = {list.Count}" )]
	public sealed class ReadonlyObservableList<T> : ThinObservableCollection, IList<T>, IList where T : class
	{
		private IList<T> list;
		public IList<T> List
		{
			get { return list; }
			set { ReplaceInnerList( value ); }
		}

		public ReadonlyObservableList( IList<T> list )
		{
			ReplaceInnerList( list );
		}

		private void ReplaceInnerList( IList<T> innerList )
		{
			if ( innerList == null )
				throw new ArgumentNullException( "innerList" );

			list = innerList;

			RaiseCollectionReset();
		}

		#region IList<T> Members

		public int IndexOf( T item )
		{
			return list.IndexOf( item );
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
				do
				{
					T item = list[index];

					if ( item != null )
					{
						return item;
					}
					Thread.Yield();
				} while ( true );
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		#endregion

		#region ICollection<T> Members

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
			get { return list.Count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public bool Remove( T item )
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IEnumerable<T> Members

		public IEnumerator<T> GetEnumerator()
		{
			return new IndexerEnumerator<T>( list );
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return list.GetEnumerator();
		}

		#endregion

		#region IList Members

		public int Add( object value )
		{
			throw new NotImplementedException();
		}

		public bool Contains( object value )
		{
			throw new NotImplementedException();
		}

		public int IndexOf( object value )
		{
			return list.IndexOf( (T)value );
		}

		public void Insert( int index, object value )
		{
			throw new NotImplementedException();
		}

		public bool IsFixedSize
		{
			get { return false; }
		}

		public void Remove( object value )
		{
			throw new NotImplementedException();
		}

		object IList.this[int index]
		{
			get
			{
				return list[index];
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		#endregion

		#region ICollection Members

		public void CopyTo( Array array, int index )
		{
			throw new NotImplementedException();
		}

		public bool IsSynchronized
		{
			get { return false; }
		}

		public object SyncRoot
		{
			get { return list; }
		}

		#endregion
	}
}
