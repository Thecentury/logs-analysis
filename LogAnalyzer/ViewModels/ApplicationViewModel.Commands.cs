using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xaml;
using LogAnalyzer.Collections;
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
			DelegateCommand command = new DelegateCommand( () => AddNewTab( logFileViewModel.Clone() ) );

			return command;
		}

		private void AddNewTab( TabViewModel tabViewModel )
		{
			var entriesTab = tabViewModel as LogEntriesListViewModel;
			if ( entriesTab != null )
			{
				entriesTab.ParentView = SelectedTab as LogEntriesListViewModel;
			}

			tabs.Add( tabViewModel );
			SelectedIndex = tabs.Count - 1;
		}

		public ICommand CreateAddDirectoryViewCommand( LogDirectoryViewModel directoryViewModel )
		{
			DelegateCommand command = new DelegateCommand( () => AddNewTab( directoryViewModel.Clone() ) );

			return command;
		}

		private DelegateCommand createFilterAndViewCommand;
		public ICommand CreateFilterAndViewCommand
		{
			get
			{
				if ( createFilterAndViewCommand == null )
					createFilterAndViewCommand = new DelegateCommand( CreateFilterAndViewCommandExecute );

				return createFilterAndViewCommand;
			}
		}

		public void CreateFilterAndViewCommandExecute()
		{
			ExpressionBuilder filterBuilder = ShowFilterEditorWindow();
			if ( filterBuilder == null )
				return;

			FilterTabViewModel filterViewModel = new FilterTabViewModel( coreViewModel.Entries, this );
			filterViewModel.Filter.ExpressionBuilder = filterBuilder;

			AddNewTab( filterViewModel );
			filterViewModel.StartFiltering();
		}

		public ExpressionBuilder ShowFilterEditorWindow()
		{
			FilterEditorWindow editorWindow = new FilterEditorWindow( Application.Current.MainWindow );
			FilterEditorViewModel editorViewModel = new FilterEditorViewModel( editorWindow );
			editorWindow.ShowDialog();
			if ( editorViewModel.DialogResult )
			{
				return editorViewModel.Builder;
			}
			else
			{
				return null;
			}
		}

		public HighlightEditorWindowViewModel ShowHighlightEditorWindow()
		{
			HighlightFilterEditorWindow window = new HighlightFilterEditorWindow();
			HighlightEditorWindowViewModel vm = new HighlightEditorWindowViewModel( window );
			window.ShowDialog();
			if ( vm.DialogResult )
			{
				return vm;
			}
			else
			{
				return null;
			}
		}

		private DelegateCommand newViewFromSavedFilterCommand;
		public ICommand NewViewFromSavedFilterCommand
		{
			get
			{
				if ( newViewFromSavedFilterCommand == null )
					newViewFromSavedFilterCommand = new DelegateCommand( NewViewFromSavedFilterCommandExecute );

				return newViewFromSavedFilterCommand;
			}
		}

		private void NewViewFromSavedFilterCommandExecute()
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

					AddNewTab( filterViewModel );
					filterViewModel.StartFiltering();
				}
			}
		}

		// Add view from core 

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

		public DelegateCommand CreateAddMessageTypeViewCommand( string messageType )
		{
			return new DelegateCommand( () =>
										{
											var filter = new MessageTypeEquals( messageType );
											AddFilterViewFromCore( filter );
										} );
		}

		#region Exclude by commands

		// Exclude file

		internal ExpressionBuilder CreateExcludeFileFilter( LogEntry logEntry )
		{
			var file = logEntry.ParentLogFile;
			var builder =
				new NotEquals(
					new GetProperty( new Argument(), "ParentLogFile" ),
					ExpressionBuilder.CreateConstant( file )
					);

			return builder;
		}

		public DelegateCommand CreateExcludeByCertainFileCommand( LogEntryViewModel logEntryViewModel )
		{
			return new DelegateCommand( () =>
											{
												var filter = CreateExcludeFileFilter( logEntryViewModel.LogEntry );

												UpdateOrAddFilterTab( logEntryViewModel, filter );
											} );
		}

		// Exclude ThreadId

		public DelegateCommand CreateExcludeByThreadIdCommand( LogEntryViewModel logEntryViewModel )
		{
			return new DelegateCommand( () =>
										{
											var filter = new ThreadIdNotEquals( logEntryViewModel.ThreadId );
											UpdateOrAddFilterTab( logEntryViewModel, filter );
										} );
		}

		// Exclude filename

		public DelegateCommand CreateExcludeByFilenameCommand( LogEntryViewModel logEntryViewModel )
		{
			return new DelegateCommand( () =>
											{
												var filter = new FileNameNotEquals( logEntryViewModel.File.Name );
												UpdateOrAddFilterTab( logEntryViewModel, filter );
											} );
		}

		// Exclude directory

		public DelegateCommand CreateExcludeDirectoryCommand( LogEntryViewModel logEntryViewModel )
		{
			return new DelegateCommand( () =>
										{
											ExpressionBuilder filter = new NotEquals(
												new GetProperty(
													new GetProperty(
														new GetProperty( new Argument(), "ParentLogFile" ),
														"ParentDirectory" ),
													"Path"
													),
												new StringConstant( logEntryViewModel.Directory.Path )
												);
											UpdateOrAddFilterTab( logEntryViewModel, filter );
										} );
		}

		// Exclude by message type

		public DelegateCommand CreateExcludeByMessageTypeCommand( LogEntryViewModel logEntryViewModel )
		{
			return new DelegateCommand( () =>
											{
												var filter = new Not( new MessageTypeEquals( logEntryViewModel.Type ) );
												UpdateOrAddFilterTab( logEntryViewModel, filter );
											} );
		}

		// Вспомогательные

		private void UpdateOrAddFilterTab( LogEntryViewModel logEntryViewModel, ExpressionBuilder filter )
		{
			FilterTabViewModel filterTab = logEntryViewModel.ParentViewModel as FilterTabViewModel;
			if ( filterTab != null )
			{
				AddExcludeByFilter( filterTab, filter );
			}
			else
			{
				var selectedTab = tabs.Single( t => t.IsActive ) as LogEntriesListViewModel;
				if ( selectedTab != null )
				{
					FilterTabViewModel filterViewModel = new FilterTabViewModel( selectedTab.Entries, this, filter );

					AddNewTab( filterViewModel );
					filterViewModel.StartFiltering();
				}
				else
				{
					AddFilterViewFromCore( filter );
				}
			}
		}

		private void AddExcludeByFilter( FilterTabViewModel filterTab, ExpressionBuilder filter )
		{
			var current = filterTab.Filter.ExpressionBuilder;

			AndCollection and = current as AndCollection;
			if ( and != null )
			{
				and.Children.Add( filter );
			}
			else
			{
				and = new AndCollection();
				and.Children.Add( current );
				and.Children.Add( filter );
				filterTab.Filter.ExpressionBuilder = and;
			}

			//filterTab.StartFiltering();
		}

		#endregion

		public DelegateCommand CreateShowInParentViewCommand( LogEntryViewModel logEntryViewModel, LogEntriesListViewModel parentTab )
		{
			return new DelegateCommand( () =>
											{
												int index = ParallelHelper.SequentialIndexOf( parentTab.Entries, logEntryViewModel.LogEntry );
												SelectedIndex = tabs.IndexOf( parentTab );
												parentTab.SelectedEntryIndex = index;
											} );
		}
	}
}
