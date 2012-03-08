using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;
using LogAnalyzer.GUI.Common;

namespace LogAnalyzer.GUI.ViewModels.FilesTree
{
	public sealed class DirectoryTreeItem : FileTreeItemBase
	{
		private readonly LogDirectory directory;
		private readonly ObservableCollection<FileTreeItem> files;

		public DirectoryTreeItem( [NotNull] LogDirectory directory )
		{
			if ( directory == null ) throw new ArgumentNullException( "directory" );

			this.directory = directory;
			directory.Files.CollectionChanged += OnFiles_CollectionChanged;
			files = new ObservableCollection<FileTreeItem>( directory.Files.Select( file => new FileTreeItem( file ) ).OrderBy( f => f.Header ) );

			foreach ( var file in files )
			{
				file.PropertyChanged += OnFilePropertyChanged;
			}
		}

		private void OnFilePropertyChanged( object sender, PropertyChangedEventArgs e )
		{
			if ( e.PropertyName == "IsChecked" )
			{
				UpdateIsChecked();
			}
		}

		private void UpdateIsChecked()
		{
			bool allChecked = files.All( f => f.IsChecked );
			if ( allChecked )
			{
				IsChecked = true;
				return;
			}

			bool allUnchecked = files.All( f => !f.IsChecked );
			if ( allUnchecked )
			{
				IsChecked = false;
				return;
			}

			IsChecked = null;
		}

		private void OnFiles_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			// todo brinchuk implement
		}

		public override string Header
		{
			get { return directory.DisplayName; }
		}

		public override string IconSource
		{
			get { return PackUriHelper.MakePackUri( "/Resources/folder.png" ); }
		}

		private bool? _isChecked;
		public bool? IsChecked
		{
			get { return _isChecked; }
			set
			{
				if ( _isChecked == value )
					return;

				_isChecked = value;

				if ( value.HasValue )
				{
					bool actualValue = value.Value;
					foreach ( var file in files )
					{
						file.IsChecked = actualValue;
					}
				}

				RaisePropertyChanged( "IsChecked" );
			}
		}

		public ObservableCollection<FileTreeItem> Files
		{
			get { return files; }
		}
	}
}