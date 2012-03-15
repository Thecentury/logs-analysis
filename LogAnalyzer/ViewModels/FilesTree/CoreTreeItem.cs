using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using JetBrains.Annotations;
using LogAnalyzer.Extensions;

namespace LogAnalyzer.GUI.ViewModels.FilesTree
{
	public sealed class CoreTreeItem : BindingObject, IRequestShow
	{
		[UsedImplicitly]
		private readonly LogAnalyzerCore _core;

		private readonly IScheduler _scheduler;
		private readonly List<DirectoryTreeItem> _directories;

		public CoreTreeItem( [NotNull] LogAnalyzerCore core, [NotNull] IScheduler scheduler )
		{
			if ( core == null )
			{
				throw new ArgumentNullException( "core" );
			}
			if (scheduler == null)
			{
				throw new ArgumentNullException("scheduler");
			}

			this._core = core;
			_scheduler = scheduler;
			this._directories = new List<DirectoryTreeItem>( core.Directories.Select( CreateDirectory ) );
		}

		public IList<DirectoryTreeItem> Directories
		{
			get { return _directories; }
		}

		public event EventHandler<RequestShowEventArgs> RequestShow;

		private DirectoryTreeItem CreateDirectory( LogDirectory directory )
		{
			DirectoryTreeItem treeItem = new DirectoryTreeItem( directory, _scheduler );

			treeItem.RequestShow += OnTreeItemRequestShow;

			return treeItem;
		}

		private void OnTreeItemRequestShow( object sender, RequestShowEventArgs e )
		{
			RequestShow.Raise( this, e );
		}
	}
}