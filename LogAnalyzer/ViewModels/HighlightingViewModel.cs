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
		private readonly IList<LogEntry> _entriesSource;
		private readonly SparseLogEntryViewModelList _logEntriesViewModels;
		private readonly LogEntriesListViewModel _parentViewModel;
		private readonly IDisposable _collectionChangedSubscription;

		protected internal HighlightingViewModel( [NotNull] LogEntriesListViewModel parentViewModel,
			[NotNull] ExpressionBuilder builder )
			: this( parentViewModel, builder, null ) { }

		protected internal HighlightingViewModel( [NotNull] LogEntriesListViewModel parentViewModel,
			[NotNull] ExpressionBuilder builder, Brush brush )
		{
			if ( parentViewModel == null ) throw new ArgumentNullException( "parentViewModel" );
			if ( builder == null ) throw new ArgumentNullException( "builder" );

			this._entriesSource = parentViewModel.Entries;
			this._logEntriesViewModels = parentViewModel.LogEntriesViewModels;
			this._parentViewModel = parentViewModel;
			_logEntriesViewModels.ItemCreated += OnLogEntriesViewModelsItemCreated;
			this.Filter.ExpressionBuilder = builder;
			this._brush = brush;

			INotifyCollectionChanged observableCollection = _entriesSource as INotifyCollectionChanged;
			if ( observableCollection != null )
			{
				_collectionChangedSubscription = observableCollection.ToNotifyCollectionChangedObservable()
					.ObserveOn( DispatcherHelper.GetDispatcher() )
					.Select( o => o.EventArgs )
					.Subscribe( OnSourceCollectionChanged );
			}

			_observableFilteredEntries = new ObservableList<LogEntry>( _acceptedEntries );
			FillAcceptedEntries( _entriesSource );

			ScanCreatedEntries();

			_filter.Changed += OnFilterChanged;
		}

		private void ScanCreatedEntries()
		{
			foreach ( var logEntryViewModel in _logEntriesViewModels.CreatedEntries )
			{
				if ( _filter.Include( logEntryViewModel.LogEntry ) )
				{
					logEntryViewModel.HighlightedByList.Add( this );
				}
			}
		}

		public LogEntriesListViewModel ParentView
		{
			get { return _parentViewModel; }
		}

		private Brush _brush;
		public Brush Brush
		{
			get { return _brush; }
			set
			{
				_brush = value;
				RaisePropertyChanged( "Brush" );
			}
		}

		private readonly ExpressionFilter<LogEntry> _filter = new ExpressionFilter<LogEntry>();
		public ExpressionFilter<LogEntry> Filter
		{
			get { return _filter; }
		}

		private void OnFilterChanged( object sender, EventArgs e )
		{
			RemoveSelfFromCreatedEntries();

			_acceptedEntries.Clear();
			FillAcceptedEntries( _entriesSource );

			ScanCreatedEntries();

			RaisePropertyChanged( "Description" );
			Changed.Raise( this );
		}

		public string Description
		{
			get { return _filter.ExpressionBuilder.ToExpressionString(); }
		}

		private int _highlightedCount;
		public int HighlightedCount
		{
			get { return _highlightedCount; }
			set
			{
				if ( _highlightedCount == value )
					return;

				_highlightedCount = value;
				RaisePropertyChanged( "HighlightedCount" );
			}
		}

		// todo brinchuk is this neccesary?
		public event EventHandler Changed;

		private int _selectedAcceptedEntryIndex = -1;
		public int SelectedAcceptedEntryIndex
		{
			get { return _selectedAcceptedEntryIndex; }
			set
			{
				_selectedAcceptedEntryIndex = value;

				var entry = _acceptedEntries[value];
				int indexInParentView = ParallelHelper.SequentialIndexOf( _parentViewModel.Entries, entry );
				_parentViewModel.SelectedEntryIndex = indexInParentView;

				CurrentIndex = value + 1;
				RaisePropertyChanged( "SelectedAcceptedEntryIndex" );
			}
		}

		private int _currentIndex;
		public int CurrentIndex
		{
			get { return _currentIndex; }
			set
			{
				_currentIndex = value;
				RaisePropertyChanged( "CurrentIndex" );
			}
		}

		private readonly List<LogEntry> _acceptedEntries = new List<LogEntry>();
		private readonly ObservableList<LogEntry> _observableFilteredEntries;

		public IList<LogEntry> AcceptedEntries
		{
			get { return _observableFilteredEntries; }
		}

		private readonly object _acceptedEntriesSync = new object();

		private void OnSourceCollectionChanged( NotifyCollectionChangedEventArgs e )
		{
			if ( e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null )
			{
				List<LogEntry> acceptedAddedEntries = new List<LogEntry>();
				foreach ( LogEntry added in e.NewItems )
				{
					if ( _filter.Include( added ) )
					{
						acceptedAddedEntries.Add( added );
					}
				}

				lock ( _acceptedEntriesSync )
				{
					int startingIndex = _acceptedEntries.Count;

					_acceptedEntries.AddRange( acceptedAddedEntries );
					_observableFilteredEntries.RaiseGenericCollectionItemsAdded( acceptedAddedEntries, startingIndex );
					_observableFilteredEntries.RaiseCountChanged();
				}
			}
			else if ( e.Action == NotifyCollectionChangedAction.Reset )
			{
				lock ( _acceptedEntriesSync )
				{
					_acceptedEntries.Clear();
					FillAcceptedEntries( _entriesSource );
					_observableFilteredEntries.RaiseCollectionReset();
					_observableFilteredEntries.RaiseCountChanged();
				}
			}
		}

		private void FillAcceptedEntries( IEnumerable<LogEntry> entries )
		{
			var added = new List<LogEntry>();

			int startingIndex = _acceptedEntries.Count;

			foreach ( var logEntry in entries )
			{
				if ( _filter.Include( logEntry ) )
				{
					_acceptedEntries.Add( logEntry );
					added.Add( logEntry );
				}
			}
			this._observableFilteredEntries.RaiseCollectionItemsAdded( added, startingIndex );
		}

		private void OnLogEntriesViewModelsItemCreated( object sender, LogEntryHostChangedEventArgs e )
		{
			if ( _filter.Include( e.LogEntryViewModel.LogEntry ) )
			{
				e.LogEntryViewModel.HighlightedByList.Add( this );
			}
		}

		public override void Dispose()
		{
			base.Dispose();
			_collectionChangedSubscription.Dispose();
			_logEntriesViewModels.ItemCreated -= OnLogEntriesViewModelsItemCreated;
			RemoveSelfFromCreatedEntries();
		}

		private void RemoveSelfFromCreatedEntries()
		{
			foreach ( var createdEntry in _logEntriesViewModels.CreatedEntries )
			{
				createdEntry.HighlightedByList.Remove( this );
			}
		}

		#region Commands

		// Show editor

		private DelegateCommand _showEditorCommand;
		public ICommand ShowEditorCommand
		{
			get
			{
				if ( _showEditorCommand == null )
					_showEditorCommand = new DelegateCommand( ShowEditorExecute, ShowEditorCanExecute );

				return _showEditorCommand;
			}
		}

		private void ShowEditorExecute()
		{
			var vm = _parentViewModel.ApplicationViewModel.ShowHighlightEditorWindow( this );
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

		private DelegateCommand _removeHighlightingCommand;
		public ICommand RemoveHighlightingCommand
		{
			get
			{
				if ( _removeHighlightingCommand == null )
					_removeHighlightingCommand = new DelegateCommand( RemoveHighlightingExecute, RemoveHighlightingCanExecute );
				return _removeHighlightingCommand;
			}
		}

		private void RemoveHighlightingExecute()
		{
			_parentViewModel.HighlightingFilters.Remove( this );
		}

		protected virtual bool RemoveHighlightingCanExecute()
		{
			return true;
		}

		// Move to first highlighted

		private DelegateCommand _moveToFirstHighlightedCommand;
		public ICommand MoveToFirstHighlightedCommand
		{
			get
			{
				if ( _moveToFirstHighlightedCommand == null )
					_moveToFirstHighlightedCommand = new DelegateCommand( MoveToFirstHighlightedExecute, MoveToFirstHighlightedCanExecute );

				return _moveToFirstHighlightedCommand;
			}
		}

		private void MoveToFirstHighlightedExecute()
		{
			SelectedAcceptedEntryIndex = 0;
		}

		private bool MoveToFirstHighlightedCanExecute()
		{
			bool canExecute = _acceptedEntries.Count > 0 && _selectedAcceptedEntryIndex != 0;
			return canExecute;
		}

		// Move to last highlighted

		private DelegateCommand _moveToLastHighlightedCommand;
		public ICommand MoveToLastHighlightedCommand
		{
			get
			{
				if ( _moveToLastHighlightedCommand == null )
					_moveToLastHighlightedCommand = new DelegateCommand( MoveToLastHighlightedExecute, MoveToLastHighlightedCanExecute );

				return _moveToLastHighlightedCommand;
			}
		}

		private void MoveToLastHighlightedExecute()
		{
			SelectedAcceptedEntryIndex = _acceptedEntries.Count - 1;
		}

		private bool MoveToLastHighlightedCanExecute()
		{
			bool canExecute = _acceptedEntries.Count > 0 && _selectedAcceptedEntryIndex != _acceptedEntries.Count - 1;
			return canExecute;
		}

		// Move to next highlighted

		private DelegateCommand _moveToNextHighlightedCommand;
		public ICommand MoveToNextHighlightedCommand
		{
			get
			{
				if ( _moveToNextHighlightedCommand == null )
					_moveToNextHighlightedCommand = new DelegateCommand( MoveToNextHighlightedExecute, MoveToNextHighlightedCanExecute );

				return _moveToNextHighlightedCommand;
			}
		}

		private void MoveToNextHighlightedExecute()
		{
			SelectedAcceptedEntryIndex++;
		}

		private bool MoveToNextHighlightedCanExecute()
		{
			return _acceptedEntries.Count > 0 && _selectedAcceptedEntryIndex < _acceptedEntries.Count - 1;
		}

		// Move to previous highlighted

		private DelegateCommand _moveToPreviousHighlightedCommand;
		public ICommand MoveToPreviousHighlightedCommand
		{
			get
			{
				if ( _moveToPreviousHighlightedCommand == null )
					_moveToPreviousHighlightedCommand = new DelegateCommand( MoveToPreviousHighlightedExecute, MoveToPreviousHighlightedCanExecute );

				return _moveToPreviousHighlightedCommand;
			}
		}

		private void MoveToPreviousHighlightedExecute()
		{
			SelectedAcceptedEntryIndex--;
		}

		private bool MoveToPreviousHighlightedCanExecute()
		{
			return _acceptedEntries.Count > 0 && _selectedAcceptedEntryIndex > 0;
		}

		#endregion
	}
}
