using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using JetBrains.Annotations;
using LogAnalyzer.GUI.Common;

namespace LogAnalyzer.GUI.ViewModels.FilesTree
{
	public abstract class FileTreeItemBase : BindingObject
	{
		public abstract string Header { get; }

		public abstract string IconSource { get; }
	}

	public sealed class FileTreeItem : FileTreeItemBase
	{
		private readonly LogFile logFile;

		public FileTreeItem( [NotNull] LogFile logFile )
		{
			if ( logFile == null ) throw new ArgumentNullException( "logFile" );
			this.logFile = logFile;
		}

		public override string Header
		{
			get { return logFile.Name; }
		}

		private static readonly Dictionary<string, string> fileNameToIconMap = new Dictionary<string, string>
		{
		    {"security", "lock"},
			{"pagebuilder-eticket_actual", "box-document"}
		};

		public override string IconSource
		{
			get
			{
				string cleanFileName = logFile.Name.ToLower().Replace( ".log", "" );
				if ( fileNameToIconMap.ContainsKey( cleanFileName ) )
				{
					string icon = fileNameToIconMap[cleanFileName];
					return PackUriHelper.MakePackUri( String.Format( "/Resources/{0}.png", icon ) );
				}

				char firstLetter = Char.ToLower( logFile.Name[0] );

				bool isLatinLetter = 'a' <= firstLetter && firstLetter <= 'z';
				if ( isLatinLetter )
				{
					string path = logFile.Name[0].ToString().ToLower();
					string uri = PackUriHelper.MakePackUri( string.Format( "/Resources/Documents/document-attribute-{0}.png", path ) );
					return uri;
				}
				else
				{
					return PackUriHelper.MakePackUri( "/Resources/Document/document-globe.png" );
				}
			}
		}

		public bool IsChecked { get; set; }
	}

	public sealed class DirectoryTreeItem : FileTreeItemBase
	{
		private readonly LogDirectory directory;
		private readonly ObservableCollection<FileTreeItem> files;

		public DirectoryTreeItem( [NotNull] LogDirectory directory )
		{
			if ( directory == null ) throw new ArgumentNullException( "directory" );

			this.directory = directory;
			directory.Files.CollectionChanged += OnFiles_CollectionChanged;
			files = new ObservableCollection<FileTreeItem>( directory.Files.Select( file => new FileTreeItem( file ) ) );
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

		public bool? IsChecked { get; set; }

		public ObservableCollection<FileTreeItem> Files
		{
			get { return files; }
		}
	}

	public sealed class CoreTreeItem : BindingObject
	{
		private readonly LogAnalyzerCore core;
		private readonly List<DirectoryTreeItem> directories;

		public CoreTreeItem( [NotNull] LogAnalyzerCore core )
		{
			if ( core == null ) throw new ArgumentNullException( "core" );
			this.core = core;
			this.directories = new List<DirectoryTreeItem>( core.Directories.Select( dir => new DirectoryTreeItem( dir ) ) );
		}

		public IList<DirectoryTreeItem> Directories
		{
			get { return directories; }
		}
	}
}
