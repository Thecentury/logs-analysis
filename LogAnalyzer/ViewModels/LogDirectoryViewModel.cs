using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reactive.Concurrency;
using System.Windows;
using System.Windows.Input;
using LogAnalyzer.Extensions;
using LogAnalyzer.GUI.Common;
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

		private readonly MessageSeverityCountViewModel messageSeverityCount;
		public override MessageSeverityCountViewModel MessageSeverityCount
		{
			get { return messageSeverityCount; }
		}

		private readonly DispatcherObservableCollection syncronizedFilesViewModels;

		public LogDirectoryViewModel( LogDirectory directory, CoreViewModel coreViewModel )
			: base( coreViewModel.ApplicationViewModel )
		{
			if ( directory == null )
				throw new ArgumentNullException( "directory" );
			if ( coreViewModel == null )
				throw new ArgumentNullException( "coreViewModel" );

			this.directory = directory;
			this.coreViewModel = coreViewModel;

			filesViewModels = new BatchUpdatingObservableCollection<LogFileViewModel>( directory.Files.Select( f => new LogFileViewModel( f, this ) ) );
			directory.Files.CollectionChanged += Files_CollectionChanged;

			Init( directory.MergedEntries );

			syncronizedFilesViewModels = new DispatcherObservableCollection( filesViewModels, coreViewModel.Scheduler );
			// произойдет уже в этом потоке
			syncronizedFilesViewModels.CollectionChanged += OnFilesViewModelsViewModelsCollectionChanged;

			this.messageSeverityCount = new MessageSeverityCountViewModel( directory.MessageSeverityCount );
		}

		private void Files_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			if ( e.Action == NotifyCollectionChangedAction.Reset )
			{
				filesViewModels.RaiseCollectionReset();
				return;
			}

			if ( e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null )
			{
				foreach ( LogFile addedFile in e.NewItems )
				{
					LogFileViewModel fileViewModel = new LogFileViewModel( addedFile, this );
					filesViewModels.Add( fileViewModel );
				}
				return;
			}
		}

		protected internal override LogFileViewModel GetFileViewModel( LogEntry logEntry )
		{
			LogFileViewModel result = filesViewModels.First( vm => vm.LogFile == logEntry.ParentLogFile );
			return result;
		}

		private void OnFilesViewModelsViewModelsCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			if ( e.Action != NotifyCollectionChangedAction.Add )
				return;

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
