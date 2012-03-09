using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LogAnalyzer.Extensions;

namespace LogAnalyzer.GUI.ViewModels.FilesTree
{
	public sealed class CoreTreeItem : BindingObject, IRequestShow
	{
		[UsedImplicitly]
		private readonly LogAnalyzerCore _core;
		private readonly List<DirectoryTreeItem> _directories;

		public CoreTreeItem( [NotNull] LogAnalyzerCore core )
		{
			if ( core == null )
			{
				throw new ArgumentNullException( "core" );
			}

			this._core = core;
			this._directories = new List<DirectoryTreeItem>( core.Directories.Select( CreateDirectory ) );
		}

		public IList<DirectoryTreeItem> Directories
		{
			get { return _directories; }
		}

		public event EventHandler<RequestShowEventArgs> RequestShow;

		private DirectoryTreeItem CreateDirectory( LogDirectory directory )
		{
			DirectoryTreeItem treeItem = new DirectoryTreeItem( directory );

			treeItem.RequestShow += OnTreeItemRequestShow;

			return treeItem;
		}

		private void OnTreeItemRequestShow( object sender, RequestShowEventArgs e )
		{
			RequestShow.Raise( this, e );
		}
	}
}