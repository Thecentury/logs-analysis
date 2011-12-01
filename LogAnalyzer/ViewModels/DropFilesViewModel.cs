﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using JetBrains.Annotations;
using LogAnalyzer.Config;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.GUI.ViewModels
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

			directoryInfo = PredefinedFilesDirectoryFactory.CreateDirectory( directoryConfig, files.Select( f => f.FileName ) );
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

		private readonly ObservableCollection<DroppedFileViewModel> files = new ObservableCollection<DroppedFileViewModel>();
		public IList<DroppedFileViewModel> Files
		{
			get { return files; }
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
						throw new NotImplementedException();
					}
				}
			}
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
			files.Clear();
		}


		#endregion
	}
}
