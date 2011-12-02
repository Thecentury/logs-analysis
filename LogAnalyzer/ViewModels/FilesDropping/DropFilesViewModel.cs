using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using JetBrains.Annotations;
using LogAnalyzer.Config;
using LogAnalyzer.Extensions;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.GUI.ViewModels.FilesDropping
{
	public sealed class DropFilesViewModel : TabViewModel
	{
		private readonly IFileSystem fileSystem;
		private readonly IDirectoryInfo directoryInfo;
		private readonly LogDirectory logDirectory;

		public DropFilesViewModel( [NotNull] ApplicationViewModel applicationViewModel, [NotNull] IFileSystem fileSystem )
			: base( applicationViewModel )
		{
			this.fileSystem = fileSystem;
			if ( applicationViewModel == null ) throw new ArgumentNullException( "applicationViewModel" );
			if ( fileSystem == null ) throw new ArgumentNullException( "fileSystem" );

			var directoryConfig = new LogDirectoryConfigurationInfo( "DroppedFiles", "*", "DroppedFiles" );

			directoryInfo = PredefinedFilesDirectoryFactory.CreateDirectory( directoryConfig, files.Select( f => f.Name ) );
			applicationViewModel.Environment.Directories.Add( directoryInfo );

			applicationViewModel.Config.Directories.Add( directoryConfig );

			files.CollectionChanged += OnFilesCollectionChanged;

			logDirectory = applicationViewModel.Core.Directories.First( d => d.DirectoryConfig == directoryConfig );
		}

		private void OnFilesCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			RaisePropertyChanged( "IconFile" );
			RaisePropertyChanged( "HasNoFiles" );
			RaisePropertyChanged( "HasFiles" );
		}

		protected override bool CanBeClosedCore()
		{
			return false;
		}

		public override string Header
		{
			get
			{
				return "Drop files";
			}
		}

		public override string IconFile
		{
			get
			{
				string icon = files.Count > 0 ? "battery-charge" : "battery-low";

				return MakePackUri( string.Format( "/Resources/{0}.png", icon ) );
			}
		}

		public bool HasNoFiles
		{
			get { return files.Count == 0; }
		}

		public bool HasFiles
		{
			get { return files.Count > 0; }
		}

		private readonly ObservableCollection<DroppedObjectViewModel> files = new ObservableCollection<DroppedObjectViewModel>();
		public IList<DroppedObjectViewModel> Files
		{
			get { return files; }
		}

		public override void Dispose()
		{
			foreach ( var file in Files )
			{
				file.Dispose();
			}
		}

		#region Commands

		// Drop Command

		private DelegateCommand<IDataObject> dropCommand;
		public DelegateCommand<IDataObject> DropCommand
		{
			get
			{
				if ( dropCommand == null )
					dropCommand = new DelegateCommand<IDataObject>( DropCommandExecute );

				return dropCommand;
			}
		}

		private void DropCommandExecute( [NotNull] IDataObject data )
		{
			if ( data == null ) throw new ArgumentNullException( "data" );

			if ( data.GetFormats().Contains( "FileDrop" ) )
			{
				string[] paths = (string[])data.GetData( "FileDrop" );
				foreach ( string path in paths )
				{
					if ( fileSystem.FileExists( path ) )
					{
						AddDroppedFile( path );
					}
					else if ( fileSystem.DirectoryExists( path ) )
					{
						AddDroppedDir( path );
					}
				}
			}
		}

		public DroppedDirectoryViewModel AddDroppedDir( string path )
		{
			var config = ApplicationViewModel.Config;

			string name = Path.GetDirectoryName( path );
			LogDirectoryConfigurationInfo dirConfig = new LogDirectoryConfigurationInfo( path, "*", name );
			dirConfig.EncodingName = config.DefaultEncodingName;

			var dirFactory = config.ResolveNotNull<IDirectoryFactory>();
			IDirectoryInfo dirInfo = dirFactory.CreateDirectory( dirConfig );
			var env = ApplicationViewModel.Environment;
			env.Directories.Add( dirInfo );

			var core = ApplicationViewModel.Core;
			LogDirectory dir = new LogDirectory( dirConfig, config, env, core );
			core.AddDirectory( dir );

			DroppedDirectoryViewModel droppedDir = new DroppedDirectoryViewModel( dir, dirInfo, files );
			files.Add( droppedDir );

			return droppedDir;
		}

		public DroppedFileViewModel AddDroppedFile( string path )
		{
			var file = directoryInfo.GetFileInfo( path );
			DroppedFileViewModel fileViewModel = new DroppedFileViewModel( file, logDirectory, path, files );
			files.Add( fileViewModel );

			return fileViewModel;
		}

		// Clear command

		private DelegateCommand clearCommand;
		public ICommand ClearCommand
		{
			get
			{
				if ( clearCommand == null )
				{
					clearCommand = new DelegateCommand( ClearExecute );
				}

				return clearCommand;
			}
		}

		private void ClearExecute()
		{
			files.ForEach( f => f.Dispose() );
			files.Clear();
		}

		#endregion
	}
}
