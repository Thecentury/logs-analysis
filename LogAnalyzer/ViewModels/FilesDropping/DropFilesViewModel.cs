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
		private readonly IFileSystem _fileSystem;
		private readonly ISaveToStreamDialog _saveToStreamDialog;
		private readonly IDirectoryInfo _directoryInfo;
		private readonly LogDirectory _logDirectory;
		private readonly LogDirectoryConfigurationInfo _directoryConfig;

		public DropFilesViewModel( [NotNull] ApplicationViewModel applicationViewModel, [NotNull] IFileSystem fileSystem,
			[NotNull] ISaveToStreamDialog saveToStreamDialog )
			: base( applicationViewModel )
		{
			if ( applicationViewModel == null )
			{
				throw new ArgumentNullException( "applicationViewModel" );
			}
			if ( fileSystem == null )
			{
				throw new ArgumentNullException( "fileSystem" );
			}
			if ( saveToStreamDialog == null )
			{
				throw new ArgumentNullException( "saveToStreamDialog" );
			}
			this._fileSystem = fileSystem;
			_saveToStreamDialog = saveToStreamDialog;

			_directoryConfig = new LogDirectoryConfigurationInfo( "DroppedFiles", "*", "DroppedFiles" );

			_directoryInfo = PredefinedFilesDirectoryFactory.CreateDirectory( _directoryConfig,
				_files.OfType<DroppedFileViewModel>().Select( f => f.Name ) );

			applicationViewModel.Environment.Directories.Add( _directoryInfo );

			_files.CollectionChanged += OnFilesCollectionChanged;

			applicationViewModel.Config.Directories.Add( _directoryConfig );

			_logDirectory = applicationViewModel.Core.Directories.First( d => d.DirectoryConfig == _directoryConfig );
		}

		private void OnFilesCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			RaisePropertyChanged( "IconFile" );
			RaisePropertyChanged( "HasNoFiles" );
			RaisePropertyChanged( "HasFiles" );
			AnalyzeCommand.RaiseCanExecuteChanged();
			CommandManager.InvalidateRequerySuggested();
		}

		protected override bool CanBeClosedCore()
		{
			return false;
		}

		public override string Header
		{
			get { return "Drop files"; }
		}

		public override string IconFile
		{
			get
			{
				string icon = _files.Count > 0 ? "battery-charge" : "battery-low";
				return MakePackUri( String.Format( "/Resources/{0}.png", icon ) );
			}
		}

		public bool HasNoFiles
		{
			get { return _files.Count == 0; }
		}

		public bool HasFiles
		{
			get { return _files.Count > 0; }
		}

		private readonly ObservableCollection<DroppedObjectViewModel> _files = new ObservableCollection<DroppedObjectViewModel>();
		public IList<DroppedObjectViewModel> Files
		{
			get { return _files; }
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

		private DelegateCommand<IDataObject> _dropCommand;
		public DelegateCommand<IDataObject> DropCommand
		{
			get
			{
				if ( _dropCommand == null )
					_dropCommand = new DelegateCommand<IDataObject>( DropCommandExecute );

				return _dropCommand;
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
					if ( _fileSystem.FileExists( path ) )
					{
						AddDroppedFile( path );
					}
					else if ( _fileSystem.DirectoryExists( path ) )
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

			DroppedDirectoryViewModel droppedDir = new DroppedDirectoryViewModel( dir, dirInfo, _files );
			_files.Add( droppedDir );

			return droppedDir;
		}

		public DroppedFileViewModel AddDroppedFile( string path )
		{
			var file = _directoryInfo.GetFileInfo( path );
			DroppedFileViewModel fileViewModel = new DroppedFileViewModel( file, _logDirectory, path, _files );
			_files.Add( fileViewModel );

			return fileViewModel;
		}

		// Clear command

		private DelegateCommand _clearCommand;
		public ICommand ClearCommand
		{
			get
			{
				if ( _clearCommand == null )
				{
					_clearCommand = new DelegateCommand( ClearExecute );
				}

				return _clearCommand;
			}
		}

		private void ClearExecute()
		{
			_files.ForEach( f => f.Dispose() );
			_files.Clear();
		}

		// Analyze command

		private DelegateCommand _analyzeCommand;

		public DelegateCommand AnalyzeCommand
		{
			get
			{
				if ( _analyzeCommand == null )
				{
					_analyzeCommand = new DelegateCommand( AnalyzeExecute, AnalyzeCanExecute );
				}

				return _analyzeCommand;
			}
		}

		private void AnalyzeExecute()
		{
			if ( !_files.Any( f => f is DroppedFileViewModel ) )
			{
				ApplicationViewModel.Core.RemoveDirectory( _logDirectory );
			}

			StartAnalyzingVisitor visitor = new StartAnalyzingVisitor( _logDirectory, ApplicationViewModel.Core );
			_files.ForEach( visitor.Visit );

			Finished.Raise( this );
		}

		private bool AnalyzeCanExecute()
		{
			return _files.Count > 0;
		}

		private DelegateCommand _saveCommand;
		public DelegateCommand SaveCommand
		{
			get
			{
				if ( _saveCommand == null )
				{
					_saveCommand = new DelegateCommand( SaveExecute );
				}

				return _saveCommand;
			}
		}

		private void SaveExecute()
		{
			var stream = _saveToStreamDialog.ShowDialog();
			if ( stream == null )
			{
				return;
			}

			using ( stream )
			{
				LogAnalyzerConfiguration config = new LogAnalyzerConfiguration();
				foreach ( var dir in _files.OfType<DroppedDirectoryViewModel>() )
				{
					config.AddLogDirectory( dir.LogDirectory.DirectoryConfig );
				}
				config.SaveToStream( stream );
			}
		}

		#endregion

		public event EventHandler Finished;
	}
}
