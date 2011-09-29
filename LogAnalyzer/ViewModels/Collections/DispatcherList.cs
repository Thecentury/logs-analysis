using System;
using System.Collections.Generic;
using System.Collections;

namespace LogAnalyzer.GUI.ViewModels.Collections
{
	/// <summary>
	/// Обертка над IList + оповещение о CollectionChanged в UI-потоке.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed class DispatcherList<T> : DispatcherObservableCollection, IList<T>, IList
	{
		private readonly IList<T> list = null;
		private readonly object syncRoot = new object();

		public DispatcherList( IList<T> list )
			: base( list )
		{
			if ( list == null )
				throw new ArgumentNullException( "list" );

			this.list = list;
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
			return list.Contains( item );
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
			get { throw new NotImplementedException(); }
		}

		public bool Remove( T item )
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IEnumerable<T> Members

		public IEnumerator<T> GetEnumerator()
		{
			// todo тут можно возвращать собственный enumerator, который бежит по индексу. Тогда предположительно
			// не будет исключения, что коллекция была изменена во время ее enumerating.
			return list.GetEnumerator();
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
			T item = (T)value;

			return list.Contains( item );
		}

		public int IndexOf( object value )
		{
			throw new NotImplementedException();
		}

		public void Insert( int index, object value )
		{
			throw new NotImplementedException();
		}

		public bool IsFixedSize
		{
			get { throw new NotImplementedException(); }
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
			get { throw new NotImplementedException(); }
		}

		public object SyncRoot
		{
			// todo правильно ли это?
			get { return syncRoot; }
		}

		#endregion
	}
}
