using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
		private readonly ReadOnlyCollection<DirectoryTreeItem> _readonlyDirectories;

		/// <summary>
		/// Для тестов.
		/// </summary>
		private CoreTreeItem()
		{
			_directories = new List<DirectoryTreeItem>();
			_readonlyDirectories = _directories.AsReadOnly();
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

			_core = core;
			_scheduler = scheduler;
			_directories = new List<DirectoryTreeItem>( core.Directories.Select( CreateDirectory ) );
			_readonlyDirectories = _directories.AsReadOnly();
		}

		internal IList<DirectoryTreeItem> DirectoriesInternal
		{
			get { return _directories; }
		}

		public ReadOnlyCollection<DirectoryTreeItem> Directories
		{
			get { return _readonlyDirectories; }
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
					_showCommand = new DelegateCommand( ShowExecute, ShowCanExecute );
				}

				return _showCommand;
			}
		}

		private void ShowExecute()
		{
			RequestShow.Raise( this, new RequestShowEventArgs( this ) );
		}

		private bool ShowCanExecute()
		{
			bool hasCheckedDirectories = Directories.Any( d => d.IsChecked != false );
			return hasCheckedDirectories;
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