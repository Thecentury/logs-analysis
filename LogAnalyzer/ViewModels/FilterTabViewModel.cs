using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows;
using LogAnalyzer.Filters;
using System.Threading;
using System.Threading.Tasks;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.GUI.Views;

namespace LogAnalyzer.GUI.ViewModels
{
	public sealed class FilterTabViewModel : LogEntriesListViewModel
	{
		private readonly IList<LogEntry> source;
		private List<LogEntry> filteredEntries = new List<LogEntry>();
		private readonly ThinListWrapper<LogEntry> observableFilteredEntries;
		private CancellationTokenSource cancellationSource = new CancellationTokenSource();

		public int SourceCount
		{
			get { return source.Count; }
		}

		public FilterTabViewModel( IList<LogEntry> source, ApplicationViewModel applicationViewModel, ExpressionBuilder builder )
			: this( source, applicationViewModel )
		{
			if ( builder == null )
				throw new ArgumentNullException( "builder" );

			filter.ExpressionBuilder = builder;
		}

		public FilterTabViewModel( IList<LogEntry> source, ApplicationViewModel applicationViewModel )
			: base( applicationViewModel )
		{
			if ( source == null )
				throw new ArgumentNullException( "source" );

			this.source = source;

			observableFilteredEntries = new ThinListWrapper<LogEntry>( filteredEntries );

			Init( observableFilteredEntries );

			filter.Changed += OnFilter_Changed;
		}

		protected override EntriesCountStatusBarItem GetEntriesCountStatusBarItem()
		{
			return new FilterEntriesCountStatusBarItem( this );
		}

		private const int MaxHeaderLength = 20;

		public override string Tooltip
		{
			get { return CreateExpressionString(); }
		}

		public override string Header
		{
			get
			{
				string str = CreateExpressionString();
				if ( str.Length > MaxHeaderLength )
				{
					str = str.Substring( 0, MaxHeaderLength ) + "…";
				}
				return str;
			}
		}

		private string CreateExpressionString()
		{
			// удаляем начальные и конечные скобки
			string trimmedStart = filter.ExpressionBuilder.ToExpressionString().Trim( '(' );
			int openBracketsCount = trimmedStart.Count( c => c == '(' );
			int closingBracketsCount = trimmedStart.Count( c => c == ')' );

			string expressionString = trimmedStart;

			int closingBracketsToRemove = closingBracketsCount - openBracketsCount;
			if ( closingBracketsToRemove > 0 )
			{
				expressionString = expressionString.Remove( expressionString.Length - closingBracketsToRemove );
			}

			expressionString = expressionString
				.Replace( " AndAlso ", " & " )
				.Replace( " OrElse ", " | " );

			return expressionString;
		}

		public override string IconFile
		{
			get { return MakePackUri( "/Resources/universal.png" ); }
		}

		private void OnFilter_Changed( object sender, EventArgs e )
		{
			RaisePropertyChanged( "Header" );
			StartFiltering();
			// todo handle filter changes
		}

		protected override void OnClosing()
		{
			if ( IsFiltering && cancellationSource != null )
			{
				cancellationSource.Cancel();
			}
		}

		private readonly ExpressionFilter<LogEntry> filter = new ExpressionFilter<LogEntry>();
		public ExpressionFilter<LogEntry> Filter
		{
			get { return filter; }
		}

		const int FilteringProgressNotificationsCount = 20;
		public void StartFiltering()
		{
			// todo this is temp solution
			if ( IsFiltering )
				return;

			FilteringProgress = 0;
			IsFiltering = true;
			cancellationSource = new CancellationTokenSource();
			int count = source.Count;

			Task.Factory.StartNew( () =>
			{
				FilteringResult localResult;

				int notificationStep = count / FilteringProgressNotificationsCount;
				if ( notificationStep == 0 )
				{
					// не уведомлять никогда
					notificationStep = Int32.MaxValue;
				}

				try
				{
					filteredEntries = ParallelEnumerable.Range( 0, count )
						.AsOrdered()
						.WithCancellation( cancellationSource.Token )
						.Select( i =>
						{
							if ( i % notificationStep == 0 )
							{
								FilteringProgress += 100.0 / FilteringProgressNotificationsCount;
							}

							return source[i];
						} )
						.Where( entry => filter.Include( entry ) )
						.ToList();

					observableFilteredEntries.List = filteredEntries;

					localResult = FilteringResult.Completed;
				}
				catch ( OperationCanceledException )
				{
					localResult = FilteringResult.Canceled;
				}

				Result = localResult;
				IsFiltering = false;
			} );
		}

		public void Cancel()
		{
			cancellationSource.Cancel();
		}

		private FilteringResult result = FilteringResult.NotStarted;
		public FilteringResult Result
		{
			get { return result; }
			private set
			{
				result = value;
				RaisePropertyChanged( "Result" );
			}
		}

		// todo notify about properties change
		private bool isFiltering;
		public bool IsFiltering
		{
			get { return isFiltering; }
			private set
			{
				if ( isFiltering == value )
					return;

				isFiltering = value;
				RaisePropertyChanged( "IsFiltering" );
			}
		}

		private double filteringProgress;
		public double FilteringProgress
		{
			get { return filteringProgress; }
			private set
			{
				filteringProgress = value;
				RaisePropertyChanged( "FilteringProgress" );
			}
		}

		private DelegateCommand<RoutedEventArgs> cancelFilteringCommand;
		public ICommand CancelFilteringCommand
		{
			get
			{
				if ( cancelFilteringCommand == null )
					cancelFilteringCommand = new DelegateCommand<RoutedEventArgs>( ExecuteCancelFilterCommand, CanExecuteCancelFilterCommand );

				return cancelFilteringCommand;
			}
		}

		private void ExecuteCancelFilterCommand( object state )
		{
			Cancel();
		}

		private bool CanExecuteCancelFilterCommand( object state )
		{
			return isFiltering;
		}

		protected internal override LogFileViewModel GetFileViewModel( LogEntry logEntry )
		{
			var logFileViewModel = ApplicationViewModel.CoreViewModel.GetFileViewModel( logEntry );
			return logFileViewModel;
		}

		public override LogEntriesListViewModel Clone()
		{
			throw new NotImplementedException();
		}

		#region Edit filter

		private bool editingInProgress;

		private DelegateCommand editFilterCommand;
		public ICommand EditFilterCommand
		{
			get
			{
				if ( editFilterCommand == null )
				{
					editFilterCommand = new DelegateCommand( EditFilterExecute, CanExecuteEditFilter );
				}

				return editFilterCommand;
			}
		}

		private bool CanExecuteEditFilter()
		{
			return !editingInProgress;
		}

		private void EditFilterExecute()
		{
			if ( editingInProgress )
				throw new InvalidOperationException( "Already editing" );

			FilterEditorWindow editorWindow = new FilterEditorWindow( Application.Current.MainWindow );
			FilterEditorViewModel editorViewModel = new FilterEditorViewModel( editorWindow ) { Builder = filter.ExpressionBuilder };
			editorWindow.Closed += OnEditorWindow_Closed;
			editorWindow.Show();
		}

		private void OnEditorWindow_Closed( object sender, EventArgs e )
		{
			Window window = (Window)sender;
			window.Closed -= OnEditorWindow_Closed;
			FilterEditorViewModel vm = (FilterEditorViewModel)window.DataContext;

			if ( vm.DialogResult )
			{
				// todo передавать информацию о "владельце" коллекции sourceEntries 
				// (напр., для команды ShowInParentEntriesList)
				ExpressionBuilder filterBuilder = vm.Builder;
				filter.ExpressionBuilder = filterBuilder;
			}
			editingInProgress = false;
		}

		#endregion

		private DelegateCommand refreshCommand;
		public ICommand RefreshCommand
		{
			get
			{
				if ( refreshCommand == null )
				{
					refreshCommand = new DelegateCommand( RefreshExecute, RefreshCanExecute );
				}

				return refreshCommand;
			}
		}

		public void RefreshExecute()
		{
			StartFiltering();
		}

		public bool RefreshCanExecute()
		{
			return !IsFiltering;
		}

		protected override void PopulateToolbarItems()
		{
			base.PopulateToolbarItems();
			ToolbarItems.Add( new FilterTabToolbarViewModel( this ) );
		}
	}

	public enum FilteringResult
	{
		NotStarted,
		Completed,
		Canceled
	}
}
