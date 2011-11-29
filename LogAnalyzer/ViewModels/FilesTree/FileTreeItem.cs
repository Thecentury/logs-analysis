using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
		private readonly ILogFile logFile;

		public FileTreeItem( [NotNull] ILogFile logFile )
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
			{"pagebuilder-eticket_actual", "box-document"},
			{"cacheproxy", "server-cast"},
			{"schedule", "alarm-clock-blue"},
			{"modulenotification", "at-sign"},
			{"muscat", "database"},
			{"kernel", "leaf"},
			{"corporatormanager", "briefcase"},
			{"usermanager", "users"},
			{"moduleantifraud", "user-thief-baldie"}
		};

		public static IDictionary<string, string> FileNameToIconMap
		{
			get { return fileNameToIconMap; }
		}

		private static readonly Regex startsWithDigitsRegex = new Regex( @"^\d{4}-\d{2}-\d{2}-(?<name>.*)", RegexOptions.Compiled );

		public override string IconSource
		{
			get
			{
				string logFileName = logFile.Name;

				Match match = startsWithDigitsRegex.Match( logFileName );
				if ( match.Success )
				{
					logFileName = match.Groups["name"].Value;
				}

				string cleanFileName = logFileName.ToLower().Replace( ".log", "" );
				if ( fileNameToIconMap.ContainsKey( cleanFileName ) )
				{
					string icon = fileNameToIconMap[cleanFileName];
					return PackUriHelper.MakePackUri( String.Format( "/Resources/{0}.png", icon ) );
				}

				return PackUriHelper.MakePackUri( "/Resources/document-globe.png" );
			}
		}

		private bool isChecked;
		public bool IsChecked
		{
			get { return isChecked; }
			set
			{
				isChecked = value;
				RaisePropertyChanged( "IsChecked" );
			}
		}
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

		private bool? isChecked;
		public bool? IsChecked
		{
			get { return isChecked; }
			set
			{
				if ( isChecked == value )
					return;

				isChecked = value;

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
