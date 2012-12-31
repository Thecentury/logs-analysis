using System;
using System.Collections.Generic;
using System.Windows.Input;
using JetBrains.Annotations;
using LogAnalyzer.Filters;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.GUI.ViewModels.FilesDropping
{
	public sealed class DroppedDirectoryViewModel : DroppedObjectViewModel
	{
		private readonly LogDirectory _directory;
		private IDirectoryInfo _dirInfo;
		private readonly ApplicationViewModel _applicationViewModel;

		public DroppedDirectoryViewModel(
			[NotNull] LogDirectory directory, [NotNull] IDirectoryInfo dirInfo,
			[NotNull] ICollection<DroppedObjectViewModel> parentCollection,
			[NotNull] ApplicationViewModel applicationViewModel )
			: base( parentCollection )
		{
			if ( directory == null )
			{
				throw new ArgumentNullException( "directory" );
			}
			if ( dirInfo == null )
			{
				throw new ArgumentNullException( "dirInfo" );
			}
			if ( applicationViewModel == null )
			{
				throw new ArgumentNullException( "applicationViewModel" );
			}

			this._directory = directory;
			this._dirInfo = dirInfo;
			_applicationViewModel = applicationViewModel;

			InitReadReporter( directory );
		}

		public override long Length
		{
			get { return LogDirectory.TotalLengthInBytes; }
		}

		public override string Name
		{
			get { return LogDirectory.Path; }
		}

		public override string Icon
		{
			get { return PackUriHelper.MakePackUri( "/Resources/folder-horizontal.png" ); }
		}

		public override bool CanBeRemoved
		{
			get { return true; }
		}

		public override bool IsDirectory
		{
			get { return true; }
		}

		public override void AcceptVisitor( IDroppedObjectVisitor visitor )
		{
			visitor.Visit( this );
		}

		public LogDirectory LogDirectory
		{
			get { return _directory; }
		}

		// Commands

		// Show files filter editor

		private DelegateCommand _showFilesFilterEditorCommand;

		public ICommand ShowFilesFilterEditorCommand
		{
			get
			{
				if ( _showFilesFilterEditorCommand == null )
				{
					_showFilesFilterEditorCommand = new DelegateCommand( ShowFilesFilterEditor );
				}

				return _showFilesFilterEditorCommand;
			}
		}

		private void ShowFilesFilterEditor()
		{
			var currentFilter = _directory.Config.GlobalFilesFilterBuilder;

			var filesFilter = _applicationViewModel.ShowFilterEditorWindow( typeof( LogFile ), currentFilter );
			if ( filesFilter != null )
			{
				_directory.LocalFileFilter = new ExpressionFilter<IFileInfo>( filesFilter );
				_directory.Config.GlobalFilesFilterBuilder = filesFilter;
			}
		}

		// Show file names filter editor

		private DelegateCommand _showFileNamesFilterEditorCommand;

		public ICommand ShowFileNamesFilterEditorCommand
		{
			get
			{
				if ( _showFileNamesFilterEditorCommand == null )
				{
					_showFileNamesFilterEditorCommand = new DelegateCommand( ShowFileNamesFilterEditor );
				}

				return _showFileNamesFilterEditorCommand;
			}
		}

		private void ShowFileNamesFilterEditor()
		{
			var currentFilter = _directory.Config.GlobalFileNamesFilterBuilder;

			var fileNamesFilter = _applicationViewModel.ShowFilterEditorWindow( typeof( string ), currentFilter );
			if ( fileNamesFilter != null )
			{
				_directory.LocalFileNameFilter = new ExpressionFilter<string>( fileNamesFilter );
				_directory.Config.GlobalFileNamesFilterBuilder = fileNamesFilter;
			}
		}
	}
}