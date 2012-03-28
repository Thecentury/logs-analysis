namespace DecompilerSample
{
	public static class ReferencesTreeItemExtensions
	{
		public static ReferencesTreeItem<T> AddChild<T>( this ReferencesTreeItem<T> tree, T item )
		{
			var child = new ReferencesTreeItem<T>( item );
			tree.Children.Add( child );
			return child;
		}
	}
}