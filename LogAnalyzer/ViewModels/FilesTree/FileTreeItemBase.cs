namespace LogAnalyzer.GUI.ViewModels.FilesTree
{
	public abstract class FileTreeItemBase : BindingObject, IFileTreeVisitable
	{
		public abstract string Header { get; }

		public abstract string IconSource { get; }

		public abstract void Accept( IFileTreeItemVisitor visitor );
	}

	public interface IFileTreeItemVisitor
	{
		void Visit( FileTreeItem file );
		void Visit( DirectoryTreeItem dir );
		void Visit( CoreTreeItem core );
	}

	public interface IFileTreeVisitable
	{
		void Accept( IFileTreeItemVisitor visitor );
	}
}