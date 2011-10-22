using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Xaml;
using AdTech.Common.WPF;
using LogAnalyzer.Filters;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.GUI.Views;
using Microsoft.Win32;

namespace LogAnalyzer.GUI.ViewModels
{
	public partial class ApplicationViewModel
	{
		public ICommand CreateAddFileViewCommand( LogFileViewModel logFileViewModel )
		{
			DelegateCommand command = new DelegateCommand( () => AddNewTab( logFileViewModel.Clone() ));

			return command;
		}

		private void AddNewTab( TabViewModel tabViewModel )
		{
			tabs.Add( tabViewModel );
			SelectedIndex = tabs.Count - 1;
		}

		public ICommand CreateAddDirectoryViewCommand( LogDirectoryViewModel directoryViewModel )
		{
			DelegateCommand command = new DelegateCommand( () => AddNewTab( directoryViewModel.Clone() ));

			return command;
		}

		private DelegateCommand createFilterAndViewCommand;
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
				FilterTabViewModel filterViewModel = new FilterTabViewModel( coreViewModel.Entries, this );
				filterViewModel.Filter.ExpressionBuilder = filterBuilder;

				AddNewTab( filterViewModel );
				filterViewModel.StartFiltering();
			}
		}

		private DelegateCommand newViewFromSavedFilterCommand;
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
					FilterTabViewModel filterViewModel = new FilterTabViewModel( selectedTab.Entries, this );
					filterViewModel.Filter.ExpressionBuilder = builder;
					filterViewModel.StartFiltering();

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
			FilterTabViewModel filterViewModel = new FilterTabViewModel( coreViewModel.Entries, this, filter );
			AddNewTab( filterViewModel );
			filterViewModel.StartFiltering();
		}

		public DelegateCommand CreateAddFileNameViewCommand( string fileName )
		{
			return new DelegateCommand( () =>
			{
				ExpressionBuilder filter = new FileNameEquals( fileName );

				AddFilterViewFromCore( filter );
			} );
		}
	}
}
