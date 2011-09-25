using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Collections;
using LogAnalyzer.Extensions;
using System.Diagnostics;

namespace LogAnalyzer
{
	/// <summary>
	/// Добавляет к ICollection поддержку INotifyCollectionChanged.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[DebuggerDisplay( "Count = {Count}" )]
	public sealed class ThinCollectionWrapper<T> : ThinObservableCollection, ICollection<T>, ICollection
	{
		private readonly ICollection<T> collection = null;
		public ICollection<T> Collection
		{
			get { return collection; }
		}

		public ThinCollectionWrapper( ICollection<T> collection )
		{
			if ( collection == null )
				throw new ArgumentNullException( "collection" );

			this.collection = collection;
		}

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
			get { return collection.Count; }
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
			return collection.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return collection.GetEnumerator();
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
			get { throw new NotImplementedException(); }
		}

		#endregion
	}
}
