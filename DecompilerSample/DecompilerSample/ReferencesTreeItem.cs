using System.Collections.Generic;
using System.Diagnostics;

namespace DecompilerSample
{
	[DebuggerDisplay( "Tree {_item}" )]
	public sealed class ReferencesTreeItem<T>
	{
		private readonly T _item;

		private readonly List<ReferencesTreeItem<T>> _children = new List<ReferencesTreeItem<T>>();

		public ReferencesTreeItem( T item )
		{
			_item = item;
		}

		public T Item
		{
			get { return _item; }
		}

		public List<ReferencesTreeItem<T>> Children
		{
			get { return _children; }
		}
	}
}