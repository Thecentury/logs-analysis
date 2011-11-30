using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
		private readonly IDirectoryInfo directoryInfo;
		private readonly LogDirectory logDirectory;

		public DropFilesViewModel( [NotNull] ApplicationViewModel applicationViewModel )
			: base( applicationViewModel )
		{
			if ( applicationViewModel == null ) throw new ArgumentNullException( "applicationViewModel" );

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
		public IEnumerable<DroppedFileViewModel> Files
		{
			get { return files; }
		} 

		#region Commands

		private DelegateCommand<DragEventArgs> dropCommand;

		public ICommand DropCommand
		{
			get
			{
				if ( dropCommand == null )
					dropCommand = new DelegateCommand<DragEventArgs>( DropCommandExecute );

				return dropCommand;
			}
		}

		private void DropCommandExecute( DragEventArgs args )
		{
			if ( args.Data.GetFormats().Contains( "FileDrop" ) )
			{
				string[] fileNames = (string[])args.Data.GetData( "FileDrop" );
				foreach ( string fileName in fileNames )
				{
					var file = directoryInfo.GetFileInfo( fileName );
					DroppedFileViewModel fileViewModel = new DroppedFileViewModel( file, logDirectory, fileName, files );
					files.Add( fileViewModel );
				}

				args.Handled = true;
			}
		}

		#endregion
	}
}
