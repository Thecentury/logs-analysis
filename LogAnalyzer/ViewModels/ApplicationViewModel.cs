using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows;
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
		private readonly LogAnalyzerConfiguration config;
		public LogAnalyzerConfiguration Config
		{
			get { return config; }
		}

		private readonly LogAnalyzerCore core;
		public LogAnalyzerCore Core
		{
			get { return core; }
		}

		private CoreViewModel coreViewModel;
		public CoreViewModel CoreViewModel
		{
			get { return coreViewModel; }
		}

		private readonly IEnvironment environment;
		public IEnvironment Environment
		{
			get { return environment; }
		}

		/// <summary>
		/// Для тестов.
		/// </summary>
		internal ApplicationViewModel() { }

		public ApplicationViewModel( [NotNull] LogAnalyzerConfiguration config, [NotNull] IEnvironment environment )
		{
			if ( config == null ) throw new ArgumentNullException( "config" );
			if ( environment == null ) throw new ArgumentNullException( "environment" );

			this.config = config;
			this.environment = environment;

			tabs.CollectionChanged += OnTabsCollectionChanged;

			core = new LogAnalyzerCore( config, environment );
			core.Loaded += OnCoreLoaded;

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

			tabs.Clear();
			Start();
		}

		private void Start()
		{
			ProgressState = Windows7Taskbar.ThumbnailProgressState.Normal;

			LoadingViewModel loadingViewModel = new LoadingViewModel( this );
			AddNewTab( loadingViewModel );

			config.Logger.WriteInfo( "Starting..." );

			core.Start();
		}

		private void OnTabsCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			if ( SelectedIndex >= tabs.Count )
			{
				SelectedIndex = tabs.Count - 1;
			}
		}

		private void OnCoreLoaded( object sender, EventArgs e )
		{
			OnCoreLoaded();
		}

		private bool onCoreLoadedCalled;
		protected virtual void OnCoreLoaded()
		{
			if ( onCoreLoadedCalled )
				return;

			onCoreLoadedCalled = true;

			coreViewModel = new CoreViewModel( core, this ) { IsActive = true };
			FilesTree = new CoreTreeItem( core );

			BeginInvokeInUIDispatcher( () =>
			{
				tabs.RemoveAt( 0 );
				tabs.Add( coreViewModel );

				ProgressValue = 0;
				ProgressState = Windows7Taskbar.ThumbnailProgressState.NoProgress;
				RaisePropertyChanged( "SelectedTab" );
			} );
		}

		private readonly ObservableCollection<TabViewModel> tabs = new ObservableCollection<TabViewModel>();
		public ObservableCollection<TabViewModel> Tabs
		{
			get { return tabs; }
		}

		private int progressValue;
		public int ProgressValue
		{
			get { return progressValue; }
			set
			{
				if ( progressValue == value )
					return;

				progressValue = value;
				RaisePropertyChanged( "ProgressValue" );
			}
		}

		private Windows7Taskbar.ThumbnailProgressState progressState;
		public Windows7Taskbar.ThumbnailProgressState ProgressState
		{
			get { return progressState; }
			set
			{
				if ( progressState == value )
					return;

				progressState = value;
				RaisePropertyChanged( "ProgressState" );
			}
		}

		private int selectedIndex;
		public int SelectedIndex
		{
			get { return selectedIndex; }
			set
			{
				selectedIndex = value;
				if ( selectedIndex < 0 )
					selectedIndex = 0;

				RaisePropertyChanged( "SelectedIndex" );
				RaisePropertyChanged( "SelectedTab" );
			}
		}

		public TabViewModel SelectedTab
		{
			get
			{
				if ( tabs.Count == 0 )
					return null;

				return tabs[selectedIndex];
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
