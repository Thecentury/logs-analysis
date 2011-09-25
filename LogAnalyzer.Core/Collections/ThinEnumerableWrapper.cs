using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using LogAnalyzer.Extensions;

namespace LogAnalyzer
{
	public sealed class ThinEnumerableWrapper<T> : ThinObservableCollection, IEnumerable<T>, INotifyCollectionChanged
	{
		private readonly IEnumerable<T> collection = null;
		public IEnumerable<T> Collection
		{
			get { return collection; }
		}

		public ThinEnumerableWrapper(IEnumerable<T> collection)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");

			this.collection = collection;
		}

		#region IEnumerable<T> Members

		public IEnumerator<T> GetEnumerator()
		{
			return collection.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return collection.GetEnumerator();
		}

		#endregion
	}
}
