using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Windows.Input;
using JetBrains.Annotations;
using LogAnalyzer.Extensions;
using LogAnalyzer.GUI.Common;

namespace LogAnalyzer.GUI.ViewModels.FilesTree
{
	public sealed class CoreTreeItem : BindingObject, IRequestShow, IFileTreeVisitable
	{
		[UsedImplicitly]
		private readonly LogAnalyzerCore _core;

		private readonly IScheduler _scheduler;
		private readonly List<DirectoryTreeItem> _directories;

		/// <summary>
		/// Для тестов.
		/// </summary>
		private CoreTreeItem()
		{
			_directories = new List<DirectoryTreeItem>();
		}

		internal static CoreTreeItem CreateEmpty()
		{
			return new CoreTreeItem();
		}

		public CoreTreeItem( [NotNull] LogAnalyzerCore core, [NotNull] IScheduler scheduler )
		{
			if ( core == null )
			{
				throw new ArgumentNullException( "core" );
			}
			if ( scheduler == null )
			{
				throw new ArgumentNullException( "scheduler" );
			}

			this._core = core;
			_scheduler = scheduler;
			this._directories = new List<DirectoryTreeItem>( core.Directories.Select( CreateDirectory ) );
		}

		public IList<DirectoryTreeItem> Directories
		{
			get { return _directories; }
		}

		// Commands

		// ShowCommand

		private DelegateCommand _showCommand;
		public ICommand ShowCommand
		{
			get
			{
				if ( _showCommand == null )
				{
					_showCommand = new DelegateCommand( ShowExecute );
				}

				return _showCommand;
			}
		}

		private void ShowExecute()
		{
			RequestShow.Raise( this, new RequestShowEventArgs( this ) );
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

		public void Accept( IFileTreeItemVisitor visitor )
		{
			visitor.Visit( this );
		}
	}
}