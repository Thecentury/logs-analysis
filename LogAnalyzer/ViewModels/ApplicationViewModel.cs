using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Windows;
using System.Windows.Threading;
using JetBrains.Annotations;
using LogAnalyzer.Config;
using System.Windows.Input;
using LogAnalyzer.GUI.Regions;
using LogAnalyzer.GUI.ViewModels.FilesDropping;
using LogAnalyzer.GUI.ViewModels.FilesTree;
using LogAnalyzer.GUI.Views;
using LogAnalyzer.Kernel;
using Microsoft.Win32;
using System.Xaml;
using LogAnalyzer.Filters;
using LogAnalyzer.Extensions;
using System.Collections.Specialized;
using Windows7.DesktopIntegration;
using LogAnalyzer.GUI.Common;

namespace LogAnalyzer.GUI.ViewModels
{
	public partial class ApplicationViewModel : BindingObject
	{
		private readonly LogAnalyzerConfiguration _config;
		public LogAnalyzerConfiguration Config
		{
			get { return _config; }
		}

		private readonly LogAnalyzerCore _core;
		public LogAnalyzerCore Core
		{
			get { return _core; }
		}

		private CoreViewModel _coreViewModel;
		public CoreViewModel CoreViewModel
		{
			get { return _coreViewModel; }
		}

		private readonly IEnvironment _environment;
		public IEnvironment Environment
		{
			get { return _environment; }
		}

		/// <summary>
		/// Для тестов.
		/// </summary>
		internal ApplicationViewModel() { }

		public ApplicationViewModel( [NotNull] LogAnalyzerConfiguration config, [NotNull] IEnvironment environment )
		{
			if ( config == null ) throw new ArgumentNullException( "config" );
			if ( environment == null ) throw new ArgumentNullException( "environment" );

			this._config = config;
			this._environment = environment;

			_tabs.CollectionChanged += OnTabsCollectionChanged;

			_core = new LogAnalyzerCore( config, environment );
			_core.Loaded += OnCoreLoaded;

			if ( config.EnabledDirectories.Any() )
			{
				Start();
			}
			else
			{
				var fileSystem = config.ResolveNotNull<IFileSystem>();
				DropFilesViewModel dropViewModel = new DropFilesViewModel( this, fileSystem );
				AddNewTab( dropViewModel );
				dropViewModel.Finished += OnDropViewModelFinished;
			}
		}

		private void OnDropViewModelFinished( object sender, EventArgs e )
		{
			DropFilesViewModel dropVm = (DropFilesViewModel)sender;
			dropVm.Finished -= OnDropViewModelFinished;
			dropVm.Dispose();

			_tabs.Clear();
			Start();
		}

		private void Start()
		{
			ProgressState = Windows7Taskbar.ThumbnailProgressState.Normal;

			LoadingViewModel loadingViewModel = new LoadingViewModel( this );
			AddNewTab( loadingViewModel );

			_config.Logger.WriteInfo( "Starting..." );

			_core.Start();
		}

		private void OnTabsCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			if ( SelectedIndex >= _tabs.Count )
			{
				SelectedIndex = _tabs.Count - 1;
			}
		}

		private void OnCoreLoaded( object sender, EventArgs e )
		{
			OnCoreLoaded();
		}

		private bool _onCoreLoadedCalled;
		protected virtual void OnCoreLoaded()
		{
			if ( _onCoreLoadedCalled )
			{
				return;
			}

			_onCoreLoadedCalled = true;

			_coreViewModel = new CoreViewModel( _core, this ) { IsActive = true };
			FilesTree = new CoreTreeItem( _core, _config.ResolveNotNull<IScheduler>() );
			FilesTree.RequestShow += OnFilesTreeRequestShow;

			BeginInvokeInUIDispatcher( () =>
			{
				_tabs.RemoveAt( 0 );
				_tabs.Add( _coreViewModel );

				ProgressValue = 0;
				ProgressState = Windows7Taskbar.ThumbnailProgressState.NoProgress;
				RaisePropertyChanged( "SelectedTab" );
			} );
		}

		private void OnFilesTreeRequestShow( object sender, RequestShowEventArgs e )
		{
			FilesTreeRequestShowVisitor visitor = new FilesTreeRequestShowVisitor( this );
			visitor.ProcessRequestShow( e.Source );
		}

		private readonly ObservableCollection<TabViewModel> _tabs = new ObservableCollection<TabViewModel>();
		public ObservableCollection<TabViewModel> Tabs
		{
			get { return _tabs; }
		}

		private int _progressValue;
		public int ProgressValue
		{
			get { return _progressValue; }
			set
			{
				if ( _progressValue == value )
					return;

				_progressValue = value;
				RaisePropertyChanged( "ProgressValue" );
			}
		}

		private Windows7Taskbar.ThumbnailProgressState _progressState;
		public Windows7Taskbar.ThumbnailProgressState ProgressState
		{
			get { return _progressState; }
			set
			{
				if ( _progressState == value )
					return;

				_progressState = value;
				RaisePropertyChanged( "ProgressState" );
			}
		}

		private int _selectedIndex;
		public int SelectedIndex
		{
			get { return _selectedIndex; }
			set
			{
				_selectedIndex = value;
				if ( _selectedIndex < 0 )
					_selectedIndex = 0;

				RaisePropertyChanged( "SelectedIndex" );
				RaisePropertyChanged( "SelectedTab" );
			}
		}

		public TabViewModel SelectedTab
		{
			get
			{
				if ( _tabs.Count == 0 )
					return null;

				return _tabs[_selectedIndex];
			}
		}

		#region Files tree

		private CoreTreeItem filesTree;
		public CoreTreeItem FilesTree
		{
			get { return filesTree; }
			private set
			{
				filesTree = value;
				RaisePropertyChanged( "FilesTree" );
			}
		}

		#endregion
	}
}
