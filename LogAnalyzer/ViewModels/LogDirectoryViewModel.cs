using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using AdTech.Common.WPF;
using System.Windows.Input;
using LogAnalyzer.Extensions;
using LogAnalyzer.GUI.ViewModels.Collections;

namespace LogAnalyzer.GUI.ViewModels
{
	public sealed class LogDirectoryViewModel : LogEntriesListViewModel, IHierarchyMember<CoreViewModel, LogDirectory>
	{
		private readonly LogDirectory directory;

		private readonly CoreViewModel coreViewModel;
		public CoreViewModel CoreViewModel
		{
			get { return coreViewModel; }
		}

		private readonly BatchUpdatingObservableCollection<LogFileViewModel> filesViewModels;
		public ObservableCollection<LogFileViewModel> Files
		{
			get { return filesViewModels; }
		}

		public LogDirectory LogDirectory
		{
			get { return directory; }
		}

		public string Path
		{
			get { return directory.Path; }
		}

		public string DisplayName
		{
			get { return directory.DisplayName; }
		}

		private readonly DispatcherObservableCollection syncronizedFiles;

		public LogDirectoryViewModel( LogDirectory directory, CoreViewModel coreViewModel )
			: base( coreViewModel.ApplicationViewModel )
		{
			if ( directory == null )
				throw new ArgumentNullException( "directory" );
			if ( coreViewModel == null )
				throw new ArgumentNullException( "coreViewModel" );

			this.directory = directory;
			this.coreViewModel = coreViewModel;

			this.filesViewModels = new BatchUpdatingObservableCollection<LogFileViewModel>( directory.Files.Select( f => new LogFileViewModel( f, this ) ) );

			this.syncronizedFiles = new DispatcherObservableCollection( filesViewModels );

			Init( directory.MergedEntries );

			// произойдет уже в этом потоке
			this.syncronizedFiles.CollectionChanged += OnFilesCollectionChanged;
		}

		protected internal override LogFileViewModel GetFileViewModel( LogEntry logEntry )
		{
			LogFileViewModel result = filesViewModels.First( vm => vm.LogFile == logEntry.ParentLogFile );
			return result;
		}

		private void OnFilesCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			// todo implement
			throw new NotImplementedException();

			if ( e.Action != NotifyCollectionChangedAction.Add )
				throw new NotImplementedException();

			if ( e.NewItems != null )
			{
				foreach ( LogFile addedFile in e.NewItems )
				{
					LogFileViewModel fileViewModel = new LogFileViewModel( addedFile, this );
					filesViewModels.Add( fileViewModel );
				}
			}

			BeginInvokeInUIDispatcher( () =>
			{
				filesViewModels.RaiseCollectionReset();
			} );
		}

		#region Commands

		private DelegateCommand<RoutedEventArgs> openFolderCommand;
		public ICommand OpenFolderCommand
		{
			get
			{
				if ( openFolderCommand == null )
				{
					openFolderCommand = new DelegateCommand<RoutedEventArgs>( _ =>
					{
						WindowsInterop.SelectInExplorer( directory.Path );
					} );
				}

				return openFolderCommand;
			}
		}

		#endregion

		public string OpenFolderInExplorerCommandHeader
		{
			get { return "Open \"{0}\" in Explorer".Format2( DisplayName ); }
		}

		public override LogEntriesListViewModel Clone()
		{
			LogDirectoryViewModel clone = new LogDirectoryViewModel( directory, coreViewModel );
			return clone;
		}

		public override string Header
		{
			get
			{
				return "Directory \"{0}\"".Format2( DisplayName );
			}
		}

		public override string IconFile
		{
			get
			{
				return MakePackUri( "/Resources/folder.png" );
			}
		}

		CoreViewModel IHierarchyMember<CoreViewModel, LogDirectory>.Parent
		{
			get { return coreViewModel; }
		}

		LogDirectory IHierarchyMember<CoreViewModel, LogDirectory>.Data
		{
			get { return directory; }
		}
	}
}
