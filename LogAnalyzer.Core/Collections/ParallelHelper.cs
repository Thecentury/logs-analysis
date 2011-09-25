using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.Collections
{
	public static class ParallelHelper
	{
		public static int IndexOf<T>( IList<T> collection, T item )
		{
			if ( item == null )
				throw new ArgumentNullException( "item" );

			ListItem<T> foundItem = ParallelEnumerable.Range( 0, collection.Count )
				.Select( i => new ListItem<T> { Index = i, Item = collection[i] } )
				.FirstOrDefault( i => Object.ReferenceEquals( i.Item, item ) );

			int index = foundItem.IsNull ? -1 : foundItem.Index;

			return index;
		}

		private struct ListItem<T>
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
