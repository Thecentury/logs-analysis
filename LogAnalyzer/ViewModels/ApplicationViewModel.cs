using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows;
using AdTech.Common.WPF;
using LogAnalyzer.Config;
using System.Windows.Input;
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
	// todo сделать настраиваемую ширину последней колонки DataGrid'а

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

		private readonly IWindowService windowService;

		public ApplicationViewModel( LogAnalyzerConfiguration config, IEnvironment environment )
		{
			if ( config == null ) throw new ArgumentNullException( "config" );
			if ( environment == null ) throw new ArgumentNullException( "environment" );

			this.config = config;

			config.Logger.WriteInfo( "Starting..." );

			core = new LogAnalyzerCore( config, environment );

			core.Loaded += OnCore_Loaded;

			windowService = config.ResolveNotNull<IWindowService>();
			windowService.SetProgressState( Windows7Taskbar.ThumbnailProgressState.Normal );

			LoadingViewModel loadingViewModel = new LoadingViewModel( this );
			AddNewTab( loadingViewModel );
			tabs.CollectionChanged += OnTabs_CollectionChanged;

			core.Start();
		}

		private void OnTabs_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			if ( SelectedIndex >= tabs.Count )
			{
				SelectedIndex = tabs.Count - 1;
			}
		}

		private void OnCore_Loaded( object sender, EventArgs e )
		{
			coreViewModel = new CoreViewModel( core, this ) { IsActive = true };

			BeginInvokeInUIDispatcher( () =>
			{
				tabs.RemoveAt( 0 );
				tabs.Add( coreViewModel );

				windowService.SetProgressValue( 0 );
				windowService.SetProgressState( Windows7Taskbar.ThumbnailProgressState.NoProgress );
			} );
		}

		private readonly ObservableCollection<TabViewModel> tabs = new ObservableCollection<TabViewModel>();
		public ObservableCollection<TabViewModel> Tabs
		{
			get { return tabs; }
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
	}
}
