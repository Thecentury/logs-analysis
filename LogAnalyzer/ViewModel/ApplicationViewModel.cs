using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows;
using AdTech.Common.WPF;
using LogAnalyzer.Config;
using LogAnalyzer.GUI.View;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Data;
using System.Diagnostics;
using Microsoft.Win32;
using System.IO;
using System.Xaml;
using LogAnalyzer.Filters;
using LogAnalyzer.Extensions;
using System.Collections.Specialized;
using LogAnalyzer.GUI.Properties;
using Windows7.DesktopIntegration;

namespace LogAnalyzer.GUI.ViewModel
{
	// todo сделать настраиваемую ширину последней колонки DatGrid'а

	public sealed class ApplicationViewModel : BindingObject
	{
		private readonly Core core = null;
		public Core Core
		{
			get { return core; }
		}

		private CoreViewModel coreViewModel = null;
		public CoreViewModel CoreViewModel
		{
			get { return coreViewModel; }
		}

		public ApplicationViewModel( LogAnalyzerConfiguration config )
		{
			if ( config == null )
				throw new ArgumentNullException( "config" );

			config.Logger.WriteInfo( "Starting..." );

			FileSystemEnvironment environment = new FileSystemEnvironment( config );
			core = new Core( config, environment );

			core.Loaded += OnCore_Loaded;

			TaskbarHelper.SetProgressState( Windows7Taskbar.ThumbnailProgressState.Normal );

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

				TaskbarHelper.SetProgressValue( 0 );
				TaskbarHelper.SetProgressState( Windows7Taskbar.ThumbnailProgressState.NoProgress );
			} );
		}

		private readonly ObservableCollection<TabViewModel> tabs = new ObservableCollection<TabViewModel>();
		public ObservableCollection<TabViewModel> Tabs
		{
			get { return tabs; }
		}

		private int selectedIndex = 0;
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

		#region Commands

		public ICommand CreateAddFileViewCommand( LogFileViewModel logFileViewModel )
		{
			DelegateCommand command = new DelegateCommand( () =>
			{
				AddNewTab( logFileViewModel.Clone() );
			} );

			return command;
		}

		private void AddNewTab( TabViewModel tabViewModel )
		{
			tabs.Add( tabViewModel );
			SelectedIndex = tabs.Count - 1;
		}

		public ICommand CreateAddDirectoryViewCommand( LogDirectoryViewModel directoryViewModel )
		{
			DelegateCommand command = new DelegateCommand( () =>
			{
				AddNewTab( directoryViewModel.Clone() );
			} );

			return command;
		}

		private DelegateCommand createFilterAndViewCommand = null;
		public ICommand CreateFilterAndViewCommand
		{
			get
			{
				if ( createFilterAndViewCommand == null )
				{
					createFilterAndViewCommand = new DelegateCommand( CreateFilterAndViewCommandExecute );
				}

				return createFilterAndViewCommand;
			}
		}

		public void CreateFilterAndViewCommandExecute()
		{
			FilterEditorWindow editorWindow = new FilterEditorWindow( Application.Current.MainWindow );
			FilterEditorViewModel editorViewModel = new FilterEditorViewModel( editorWindow );
			bool? dialogResult = editorWindow.ShowDialog();
			if ( dialogResult == true )
			{
				// todo передавать информацию о "владельце" коллекции sourceEntries 
				// (напр., для команды ShowInParentEntriesList)
				ExpressionBuilder filterBuilder = editorViewModel.Builder;
				FilterViewModel filterViewModel = new FilterViewModel( coreViewModel.Entries, this );
				filterViewModel.Filter.ExpressionBuilder = filterBuilder;

				AddNewTab( filterViewModel );
				filterViewModel.StartFiltration();
			}
		}

		private DelegateCommand newViewFromSavedFilterCommand = null;
		public ICommand NewViewFromSavedFilterCommand
		{
			get
			{
				if ( newViewFromSavedFilterCommand == null )
				{
					newViewFromSavedFilterCommand = new DelegateCommand( NewViewFromSavedFilterCommandExecute );
				}

				return newViewFromSavedFilterCommand;
			}
		}

		public void NewViewFromSavedFilterCommandExecute()
		{
			OpenFileDialog openDialog = new OpenFileDialog();
			if ( openDialog.ShowDialog( Application.Current.MainWindow ) == true )
			{
				string fileName = openDialog.FileName;

				// todo exception handling
				ExpressionBuilder builder = (ExpressionBuilder)XamlServices.Load( fileName );
				LogEntriesListViewModel selectedTab = tabs.Single( t => t.IsActive ) as LogEntriesListViewModel;

				if ( selectedTab != null )
				{
					FilterViewModel filterViewModel = new FilterViewModel( selectedTab.Entries, this );
					filterViewModel.Filter.ExpressionBuilder = builder;
					filterViewModel.StartFiltration();

					AddNewTab( filterViewModel );
				}
			}
		}

		public DelegateCommand CreateAddThreadViewCommand( int threadId )
		{
			return new DelegateCommand( () =>
			{
				ExpressionBuilder filter = new ThreadIdEquals( threadId );

				AddFilterViewFromCore( filter );
			} );
		}

		private void AddFilterViewFromCore( ExpressionBuilder filter )
		{
			FilterViewModel filterViewModel = new FilterViewModel( coreViewModel.Entries, this, filter );
			AddNewTab( filterViewModel );
			filterViewModel.StartFiltration();
		}

		public DelegateCommand CreateAddFileNameViewCommand( string fileName )
		{
			return new DelegateCommand( () =>
			{
				ExpressionBuilder filter = new FileNameEquals( fileName );

				AddFilterViewFromCore( filter );
			} );
		}

		#endregion
	}
}
