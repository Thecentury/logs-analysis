using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows.Input;
using JetBrains.Annotations;
using LogAnalyzer.Extensions;
using LogAnalyzer.GUI.Common;

namespace LogAnalyzer.GUI.ViewModels.FilesTree
{
	public sealed class DirectoryTreeItem : FileTreeItemBase, IRequestShow
	{
		private readonly LogDirectory _directory;
		private readonly ObservableCollection<FileTreeItem> _files;

		public DirectoryTreeItem( [NotNull] LogDirectory directory, [NotNull] IScheduler scheduler )
		{
			if ( directory == null )
			{
				throw new ArgumentNullException( "directory" );
			}
			if ( scheduler == null )
			{
				throw new ArgumentNullException( "scheduler" );
			}

			var observable = Observable.FromEventPattern<NotifyCollectionChangedEventArgs>( directory.Files,
																			   "CollectionChanged" );
			observable
				.ObserveOn( scheduler )
				.Subscribe( e => OnFilesCollectionChanged( e.EventArgs ) );

			this._directory = directory;
			_files = new ObservableCollection<FileTreeItem>( directory.Files.Select( CreateFile ).OrderBy( f => f.Header ) );
		}

		private FileTreeItem CreateFile( LogFile file )
		{
			var item = new FileTreeItem( file );

			item.PropertyChanged += OnFilePropertyChanged;
			item.RequestShow += OnFileRequestShow;

			return item;
		}

		private void OnFileRequestShow( object sender, RequestShowEventArgs e )
		{
			RequestShow.Raise( this, e );
		}

		private void OnFilePropertyChanged( object sender, PropertyChangedEventArgs e )
		{
			if ( e.PropertyName == "IsChecked" )
			{
				UpdateIsChecked();
			}
		}

		internal void UpdateIsChecked()
		{
			bool allChecked = _files.All( f => f.IsChecked );
			if ( allChecked )
			{
				IsChecked = true;
				return;
			}

			bool allUnchecked = _files.All( f => !f.IsChecked );
			if ( allUnchecked )
			{
				IsChecked = false;
				return;
			}

			IsChecked = null;
		}

		private void OnFilesViewModelsChanged( NotifyCollectionChangedEventArgs e )
		{
			UpdateIsChecked();
		}

		private void OnFilesCollectionChanged( NotifyCollectionChangedEventArgs e )
		{
			if ( e.NewItems != null )
			{
				foreach ( LogFile logFile in e.NewItems )
				{
					var fileModel = CreateFile( logFile );
					_files.Add( fileModel );
				}
			}

			if ( e.OldItems != null )
			{
				foreach ( LogFile logFile in e.OldItems )
				{
					var fileModel = _files.First( f => f.LogFile == logFile );
					_files.Remove( fileModel );
				}
			}

			UpdateIsChecked();
		}

		public LogDirectory LogDirectory
		{
			get { return _directory; }
		}

		public override string Header
		{
			get { return _directory.DisplayName; }
		}

		public override string IconSource
		{
			get { return PackUriHelper.MakePackUri( "/Resources/folder.png" ); }
		}

		public override void Accept( IFileTreeItemVisitor visitor )
		{
			visitor.Visit( this );
		}

		private bool? _isChecked = false;
		public bool? IsChecked
		{
			get { return _isChecked; }
			set
			{
				if ( _isChecked == value )
				{
					return;
				}

				_isChecked = value;

				if ( value.HasValue )
				{
					bool actualValue = value.Value;
					foreach ( var file in _files )
					{
						file.IsChecked = actualValue;
					}
				}

				RaisePropertyChanged( "IsChecked" );
			}
		}

		public ObservableCollection<FileTreeItem> Files
		{
			get { return _files; }
		}

		public event EventHandler<RequestShowEventArgs> RequestShow;

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
	}
}