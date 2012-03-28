using System.Collections.Generic;

namespace DecompilerSample
{
	public sealed class ChildrenFirstTraversal<T>
	{
		public IEnumerable<T> Enumerate( ReferencesTreeItem<T> tree )
		{
			Traversal traversal = new Traversal();
			traversal.Enumerate( tree );
			return traversal.Items;
		}

		private sealed class Traversal
		{
			private readonly List<T> _items = new List<T>();
			private readonly HashSet<T> _usedItems = new HashSet<T>();

			public List<T> Items
			{
				get { return _items; }
			}

			public void Enumerate( ReferencesTreeItem<T> tree )
			{
				foreach ( var child in tree.Children )
				{
					Enumerate( child );
				}

				if ( !_usedItems.Contains( tree.Item ) )
				{
					_usedItems.Add( tree.Item );
					Items.Add( tree.Item );
				}
			}
		}
	}
}