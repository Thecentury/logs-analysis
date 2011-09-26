using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Collections;
using LogAnalyzer.Extensions;

namespace LogAnalyzer
{
	/// <summary>
	/// Добавляет в IList поддержку INotifyCollectionChanged.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed class ThinListWrapper<T> : ThinObservableCollection, IList<T>, IList
	{
		private IList<T> list = null;
		public IList<T> List
		{
			get { return list; }
			set { ReplaceInnerList( value ); }
		}

		private void ReplaceInnerList( IList<T> value )
		{
			if ( value == null )
				throw new ArgumentNullException( "value" );

			list = value;

			RaiseCollectionReset();
		}

		public ThinListWrapper( IList<T> list )
		{
			ReplaceInnerList( list );
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
				return list[index];
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

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
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
