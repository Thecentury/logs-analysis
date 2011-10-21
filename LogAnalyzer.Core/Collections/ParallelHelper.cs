using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LogAnalyzer.Collections
{
	public static class ParallelHelper
	{
		public static int AssuredIndexOf<T>( IList<T> collection, T item ) where T : class
		{
			if ( item == null )
				throw new ArgumentNullException( "item" );

			do
			{
				ListItem<T> foundItem = ParallelEnumerable.Range( 0, collection.Count )
					.Select( i => new ListItem<T> { Index = i, Item = collection[i] } )
					.FirstOrDefault( i => ReferenceEquals( i.Item, item ) );

				if ( !foundItem.IsNull )
					return foundItem.Index;

			} while ( true );
		}

		public static int IndexOf<T>( IList<T> collection, T item ) where T : class
		{
			if ( item == null )
				throw new ArgumentNullException( "item" );

			ListItem<T> foundItem = ParallelEnumerable.Range( 0, collection.Count )
				.Select( i => new ListItem<T> { Index = i, Item = collection[i] } )
				.FirstOrDefault( i => ReferenceEquals( i.Item, item ) );

			int index = foundItem.IsNull ? IndexNotFound : foundItem.Index;

			return index;
		}

		public static int SequentialIndexOf<T>( IList<T> collection, T item ) where T : class
		{
			for ( int i = 0; i < collection.Count; i++ )
			{
				T currentItem = collection[i];
				if ( ReferenceEquals( item, currentItem ) )
					return i;
			}

			return IndexNotFound;
		}

		public const int IndexNotFound = -1;

		private struct ListItem<T> where T : class
		{
			public T Item;
			public int Index;

			public bool IsNull
			{
				get { return Item == null; }
			}
		}
	}
}
