using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using System.Windows;
using LogAnalyzer.Collections;
using LogAnalyzer.Extensions;
using LogAnalyzer.Filters;
using System.Threading;
using System.Threading.Tasks;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.GUI.Views;

namespace LogAnalyzer.GUI.ViewModels
{
	public sealed class FilterTabViewModel : LogEntriesListViewModel
	{
		private readonly object sourceReplaceLock = new object();
		private readonly IList<LogEntry> source;
		private readonly ReadonlyObservableList<LogEntry> observableFilteredEntries;
		private CancellationTokenSource cancellationSource = new CancellationTokenSource();

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

			INotifyCollectionChanged observable = source as INotifyCollectionChanged;
			if ( observable != null )
			{
				observable.CollectionChanged += OnSourceCollectionChanged;
			}

			observableFilteredEntries = new ReadonlyObservableList<LogEntry>( new List<LogEntry>() );

			Init( observableFilteredEntries );

			filter.Changed += OnFilter_Changed;
		}

		private void OnSourceCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			if ( e.Action == NotifyCollectionChangedAction.Add )
			{
				// тут считаем, что элементы были добавлены в конец
				var passedItems = e.NewItems.Cast<LogEntry>().Where( filter.Include ).ToList();

				observableFilteredEntries.List.AddRange( passedItems );
				observableFilteredEntries.RaiseCollectionItemsAdded( (IList)passedItems );
			}

			RaisePropertyChanged( "SourceCount" );
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

		protected override void OnTabClosing()
		{
			if ( IsFiltering && cancellationSource != null )
			{
				cancellationSource.Cancel();
			}

			INotifyCollectionChanged observable = source as INotifyCollectionChanged;
			if ( observable != null )
			{
				observable.CollectionChanged -= OnSourceCollectionChanged;
			}
		}

		public int SourceCount
		{
			get { return source.Count; }
		}

		private readonly ExpressionFilter<LogEntry> filter = new ExpressionFilter<LogEntry>();
		public ExpressionFilter<LogEntry> Filter
		{
			get { return filter; }
		}

		const int FilteringProgressNotificationsCount = 20;
		public void StartFiltering()
		{
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
					List<LogEntry> filtered = new List<LogEntry>( count );

					for ( int i = 0; i < count; i++ )
					{
						if ( i % notificationStep == 0 )
						{
							FilteringProgress += 100.0 / FilteringProgressNotificationsCount;
							if ( cancellationSource.IsCancellationRequested )
								break;
						}

						var entry = source[i];
						bool include = filter.Include( entry );
						if ( include )
						{
							filtered.Add( entry );
						}
					}

					lock ( sourceReplaceLock )
					{
						observableFilteredEntries.List = filtered;
					}

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

		protected internal override LogFileViewModel GetFileViewModel( LogEntry logEntry )
		{
			var logFileViewModel = ApplicationViewModel.CoreViewModel.GetFileViewModel( logEntry );
			return logFileViewModel;
		}

		public override LogEntriesListViewModel Clone()
		{
			throw new NotImplementedException();
		}

		protected override void PopulateToolbarItems( ICollection<object> collection )
		{
			base.PopulateToolbarItems( collection );
			collection.Add( new FilterTabToolbarViewModel( this ) );
		}

		#region Commands

		#region Cancel filtering command

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

		#endregion

		#region Edit filter command

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

		#region Refresh command

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

		#endregion

		#endregion
	}

	public enum FilteringResult
	{
		NotStarted,
		Completed,
		Canceled
	}
}
