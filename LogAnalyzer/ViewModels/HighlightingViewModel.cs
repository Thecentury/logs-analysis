using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using JetBrains.Annotations;
using LogAnalyzer.Collections;
using LogAnalyzer.Extensions;
using LogAnalyzer.Filters;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.GUI.ViewModels;
using LogAnalyzer.GUI.ViewModels.Collections;

namespace LogAnalyzer.GUI.ViewModels
{
	public class HighlightingViewModel : BindingObject
	{
		private readonly IList<LogEntry> entriesSource;
		private readonly SparseLogEntryViewModelList logEntriesViewModels;
		private readonly LogEntriesListViewModel parentViewModel;
		private readonly IDisposable collectionChangedSubscription;

		protected internal HighlightingViewModel( [NotNull] LogEntriesListViewModel parentViewModel,
			[NotNull] ExpressionBuilder builder )
			: this( parentViewModel, builder, null ) { }

		protected internal HighlightingViewModel( [NotNull] LogEntriesListViewModel parentViewModel,
			[NotNull] ExpressionBuilder builder, Brush brush )
		{
			if ( parentViewModel == null ) throw new ArgumentNullException( "parentViewModel" );
			if ( builder == null ) throw new ArgumentNullException( "builder" );

			this.entriesSource = parentViewModel.Entries;
			this.logEntriesViewModels = parentViewModel.LogEntriesViewModels;
			this.parentViewModel = parentViewModel;
			logEntriesViewModels.ItemCreated += OnLogEntriesViewModels_ItemCreated;
			this.Filter.ExpressionBuilder = builder;
			this.brush = brush;

			INotifyCollectionChanged observableCollection = entriesSource as INotifyCollectionChanged;
			if ( observableCollection != null )
			{
				collectionChangedSubscription = observableCollection.ToNotifyCollectionChangedObservable()
					.ObserveOn( DispatcherHelper.GetDispatcher() )
					.Select( o => o.EventArgs )
					.Subscribe( OnSourceCollectionChanged );
			}

			observableFilteredEntries = new ReadonlyObservableList<LogEntry>( acceptedEntries );
			FillAcceptedEntries( entriesSource );

			ScanCreatedEntries();

			filter.Changed += OnFilter_Changed;
		}

		private void ScanCreatedEntries()
		{
			foreach ( var logEntryViewModel in logEntriesViewModels.CreatedEntries )
			{
				if ( filter.Include( logEntryViewModel.LogEntry ) )
				{
					logEntryViewModel.HighlightedByList.Add( this );
				}
			}
		}

		private Brush brush;
		public Brush Brush
		{
			get { return brush; }
			set
			{
				brush = value;
				RaisePropertyChanged( "Brush" );
			}
		}

		private readonly ExpressionFilter<LogEntry> filter = new ExpressionFilter<LogEntry>();
		public ExpressionFilter<LogEntry> Filter
		{
			get { return filter; }
		}

		private void OnFilter_Changed( object sender, EventArgs e )
		{
			RemoveSelfFromCreatedEntries();

			acceptedEntries.Clear();
			FillAcceptedEntries( entriesSource );

			ScanCreatedEntries();

			RaisePropertyChanged( "Description" );
			Changed.Raise( this );
		}

		public string Description
		{
			get { return filter.ExpressionBuilder.ToExpressionString(); }
		}

		private int highlightedCount;
		public int HighlightedCount
		{
			get { return highlightedCount; }
			set
			{
				if ( highlightedCount == value )
					return;

				highlightedCount = value;
				RaisePropertyChanged( "HighlightedCount" );
			}
		}

		// todo brinchuk is this neccesary?
		public event EventHandler Changed;

		private int selectedAcceptedEntryIndex = -1;
		public int SelectedAcceptedEntryIndex
		{
			get { return selectedAcceptedEntryIndex; }
			set
			{
				selectedAcceptedEntryIndex = value;

				var entry = acceptedEntries[value];
				int indexInParentView = ParallelHelper.SequentialIndexOf( parentViewModel.Entries, entry );
				parentViewModel.SelectedEntryIndex = indexInParentView;

				CurrentIndex = value + 1;
				RaisePropertyChanged( "SelectedAcceptedEntryIndex" );
			}
		}

		private int currentIndex;
		public int CurrentIndex
		{
			get { return currentIndex; }
			set
			{
				currentIndex = value;
				RaisePropertyChanged( "CurrentIndex" );
			}
		}

		private readonly List<LogEntry> acceptedEntries = new List<LogEntry>();
		private readonly ReadonlyObservableList<LogEntry> observableFilteredEntries;

		public IList<LogEntry> AcceptedEntries
		{
			get { return observableFilteredEntries; }
		}

		private readonly object acceptedEntriesSync = new object();

		private void OnSourceCollectionChanged( NotifyCollectionChangedEventArgs e )
		{
			if ( e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null )
			{
				List<LogEntry> acceptedAddedEntries = new List<LogEntry>();
				foreach ( LogEntry added in e.NewItems )
				{
					if ( filter.Include( added ) )
					{
						acceptedAddedEntries.Add( added );
					}
				}

				lock ( acceptedEntriesSync )
				{
					acceptedEntries.AddRange( acceptedAddedEntries );
					observableFilteredEntries.RaiseGenericCollectionItemsAdded( acceptedAddedEntries );
					observableFilteredEntries.RaiseCountChanged();
				}
			}
			else if ( e.Action == NotifyCollectionChangedAction.Reset )
			{
				lock ( acceptedEntriesSync )
				{
					acceptedEntries.Clear();
					FillAcceptedEntries( entriesSource );
					observableFilteredEntries.RaiseCollectionReset();
					observableFilteredEntries.RaiseCountChanged();
				}
			}
		}

		private void FillAcceptedEntries( IEnumerable<LogEntry> entries )
		{
			var added = new List<LogEntry>();
			foreach ( var logEntry in entries )
			{
				if ( filter.Include( logEntry ) )
				{
					acceptedEntries.Add( logEntry );
					added.Add( logEntry );
				}
			}
			this.observableFilteredEntries.RaiseCollectionItemsAdded( added );
		}

		private void OnLogEntriesViewModels_ItemCreated( object sender, LogEntryHostChangedEventArgs e )
		{
			if ( filter.Include( e.LogEntryViewModel.LogEntry ) )
			{
				e.LogEntryViewModel.HighlightedByList.Add( this );
			}
		}

		public override void Dispose()
		{
			base.Dispose();
			collectionChangedSubscription.Dispose();
			logEntriesViewModels.ItemCreated -= OnLogEntriesViewModels_ItemCreated;
			RemoveSelfFromCreatedEntries();
		}

		private void RemoveSelfFromCreatedEntries()
		{
			foreach ( var createdEntry in logEntriesViewModels.CreatedEntries )
			{
				createdEntry.HighlightedByList.Remove( this );
			}
		}

		#region Commands

		// Show editor

		private DelegateCommand showEditorCommand;
		public ICommand ShowEditorCommand
		{
			get
			{
				if ( showEditorCommand == null )
					showEditorCommand = new DelegateCommand( ShowEditorExecute, ShowEditorCanExecute );

				return showEditorCommand;
			}
		}

		private void ShowEditorExecute()
		{
			var vm = parentViewModel.ApplicationViewModel.ShowHighlightEditorWindow( this );
			if ( vm == null )
				return;

			Brush = new SolidColorBrush( vm.SelectedColor );
			Filter.ExpressionBuilder = vm.SelectedBuilder;
		}

		protected virtual bool ShowEditorCanExecute()
		{
			return true;
		}

		// Remove highlighting command

		private DelegateCommand removeHighlightingCommand;
		public ICommand RemoveHighlightingCommand
		{
			get
			{
				if ( removeHighlightingCommand == null )
					removeHighlightingCommand = new DelegateCommand( RemoveHighlightingExecute, RemoveHighlightingCanExecute );
				return removeHighlightingCommand;
			}
		}

		private void RemoveHighlightingExecute()
		{
			parentViewModel.HighlightingFilters.Remove( this );
		}

		protected virtual bool RemoveHighlightingCanExecute()
		{
			return true;
		}

		// Move to first highlighted

		private DelegateCommand moveToFirstHighlightedCommand;
		public ICommand MoveToFirstHighlightedCommand
		{
			get
			{
				if ( moveToFirstHighlightedCommand == null )
					moveToFirstHighlightedCommand = new DelegateCommand( MoveToFirstHighlightedExecute, MoveToFirstHighlightedCanExecute );

				return moveToFirstHighlightedCommand;
			}
		}

		private void MoveToFirstHighlightedExecute()
		{
			SelectedAcceptedEntryIndex = 0;
		}

		private bool MoveToFirstHighlightedCanExecute()
		{
			bool canExecute = acceptedEntries.Count > 0 && selectedAcceptedEntryIndex != 0;
			return canExecute;
		}

		// Move to last highlighted

		private DelegateCommand moveToLastHighlightedCommand;
		public ICommand MoveToLastHighlightedCommand
		{
			get
			{
				if ( moveToLastHighlightedCommand == null )
					moveToLastHighlightedCommand = new DelegateCommand( MoveToLastHighlightedExecute, MoveToLastHighlightedCanExecute );

				return moveToLastHighlightedCommand;
			}
		}

		private void MoveToLastHighlightedExecute()
		{
			SelectedAcceptedEntryIndex = acceptedEntries.Count - 1;
		}

		private bool MoveToLastHighlightedCanExecute()
		{
			bool canExecute = acceptedEntries.Count > 0 && selectedAcceptedEntryIndex != acceptedEntries.Count - 1;
			return canExecute;
		}

		// Move to next highlighted

		private DelegateCommand moveToNextHighlightedCommand;
		public ICommand MoveToNextHighlightedCommand
		{
			get
			{
				if ( moveToNextHighlightedCommand == null )
					moveToNextHighlightedCommand = new DelegateCommand( MoveToNextHighlightedExecute, MoveToNextHighlightedCanExecute );

				return moveToNextHighlightedCommand;
			}
		}

		private void MoveToNextHighlightedExecute()
		{
			SelectedAcceptedEntryIndex++;
		}

		private bool MoveToNextHighlightedCanExecute()
		{
			return acceptedEntries.Count > 0 && selectedAcceptedEntryIndex < acceptedEntries.Count - 1;
		}

		// Move to previous highlighted

		private DelegateCommand moveToPreviousHighlightedCommand;
		public ICommand MoveToPreviousHighlightedCommand
		{
			get
			{
				if ( moveToPreviousHighlightedCommand == null )
					moveToPreviousHighlightedCommand = new DelegateCommand( MoveToPreviousHighlightedExecute, MoveToPreviousHighlightedCanExecute );

				return moveToPreviousHighlightedCommand;
			}
		}

		private void MoveToPreviousHighlightedExecute()
		{
			SelectedAcceptedEntryIndex--;
		}

		private bool MoveToPreviousHighlightedCanExecute()
		{
			return acceptedEntries.Count > 0 && selectedAcceptedEntryIndex > 0;
		}

		#endregion
	}
}
