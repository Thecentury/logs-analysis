namespace LogAnalyzer.GUI.ViewModels.FilesTree
{
	public abstract class FileTreeItemBase : BindingObject
	{
		public abstract string Header { get; }

		public abstract string IconSource { get; }
	}
}