using System;
using System.Collections.Generic;
using System.ComponentModel;
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
		private readonly LogDirectory _directory;

		private readonly CoreViewModel _coreViewModel;
		public CoreViewModel CoreViewModel
		{
			get { return _coreViewModel; }
		}

		private readonly BatchUpdatingObservableCollection<LogFileViewModel> _filesViewModels;
		public ObservableCollection<LogFileViewModel> Files
		{
			get { return _filesViewModels; }
		}

		public LogDirectory LogDirectory
		{
			get { return _directory; }
		}

		public string Path
		{
			get { return _directory.Path; }
		}

		public string DisplayName
		{
			get { return _directory.DisplayName; }
		}

		private readonly MessageSeverityCountViewModel _messageSeverityCount;
		public override MessageSeverityCountViewModel MessageSeverityCount
		{
			get { return _messageSeverityCount; }
		}

		public bool IsNotificationSourceEnabled
		{
			get { return _directory.NotificationsSource.IsEnabled; }
			set { _directory.NotificationsSource.SetIsEnabled( value ); }
		}

		private readonly DispatcherObservableCollection _syncronizedFilesViewModels;

		private readonly ToggleButtonViewModel _notificationsEnabledToggleButton;

		public LogDirectoryViewModel( LogDirectory directory, CoreViewModel coreViewModel )
			: base( coreViewModel.ApplicationViewModel )
		{
			if ( directory == null )
				throw new ArgumentNullException( "directory" );
			if ( coreViewModel == null )
				throw new ArgumentNullException( "coreViewModel" );

			this._directory = directory;
			this._coreViewModel = coreViewModel;

			_filesViewModels = new BatchUpdatingObservableCollection<LogFileViewModel>( directory.Files.Select( f => new LogFileViewModel( f, this ) ) );
			directory.Files.CollectionChanged += OnFilesCollectionChanged;

			Init( directory.MergedEntries );

			_syncronizedFilesViewModels = new DispatcherObservableCollection( _filesViewModels, coreViewModel.Scheduler );
			// произойдет уже в этом потоке
			_syncronizedFilesViewModels.CollectionChanged += OnFilesViewModelsViewModelsCollectionChanged;

			this._messageSeverityCount = new MessageSeverityCountViewModel( directory.MessageSeverityCount );

			directory.NotificationsSource.PropertyChanged += NotificationsSourcePropertyChanged;

			_notificationsEnabledToggleButton = new ToggleButtonViewModel(
				() => IsNotificationSourceEnabled,
				value => IsNotificationSourceEnabled = value,
				"Toggle autoscroll to bottom on updates", PackUriHelper.MakePackUri( "/Resources/control-record.png" )
				);
		}

		private void NotificationsSourcePropertyChanged( object sender, PropertyChangedEventArgs e )
		{
			_notificationsEnabledToggleButton.RaiseIsToggledChanged();
		}

		private void OnFilesCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			if ( e.Action == NotifyCollectionChangedAction.Reset )
			{
				_filesViewModels.RaiseCollectionReset();
				return;
			}

			if ( e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null )
			{
				foreach ( LogFile addedFile in e.NewItems )
				{
					LogFileViewModel fileViewModel = new LogFileViewModel( addedFile, this );
					_filesViewModels.Add( fileViewModel );
				}
				return;
			}
		}

		protected internal override LogFileViewModel GetFileViewModel( LogEntry logEntry )
		{
			LogFileViewModel result = _filesViewModels.First( vm => vm.LogFile == logEntry.ParentLogFile );
			return result;
		}

		private void OnFilesViewModelsViewModelsCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			if ( e.Action != NotifyCollectionChangedAction.Add )
			{
				return;
			}

			BeginInvokeInUIDispatcher( () => _filesViewModels.RaiseCollectionReset() );
		}

		protected override void PopulateToolbarItems( IList<object> collection )
		{
			base.PopulateToolbarItems( collection );

			collection.Insert( 1, _notificationsEnabledToggleButton );
		}

		protected override void PopulateStatusBarItems( ICollection<object> collection )
		{
			base.PopulateStatusBarItems( collection );

#if DEBUG
			collection.Add( new MergedEntriesDebugStatusBarItem( _directory.MergedEntries ) );
#endif
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
						WindowsInterop.SelectInExplorer( _directory.Path );
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
			LogDirectoryViewModel clone = new LogDirectoryViewModel( _directory, _coreViewModel );
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
			get { return _coreViewModel; }
		}

		LogDirectory IHierarchyMember<CoreViewModel, LogDirectory>.Data
		{
			get { return _directory; }
		}
	}
}
