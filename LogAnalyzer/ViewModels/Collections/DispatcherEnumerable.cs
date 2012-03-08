using System;
using System.Collections.Generic;
using System.Collections;
using System.Reactive.Concurrency;

namespace LogAnalyzer.GUI.ViewModels.Collections
{
	public sealed class DispatcherEnumerable<T> : DispatcherObservableCollection, IEnumerable<T>
	{
		private readonly IEnumerable<T> _collection;

		public DispatcherEnumerable( IEnumerable<T> collection, IScheduler scheduler )
			: base( collection, scheduler )
		{
			if ( collection == null )
				throw new ArgumentNullException( "collection" );

			this._collection = collection;
		}

		#region IEnumerable<T> Members

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return _collection.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _collection.GetEnumerator();
		}

		#endregion
	}
}
