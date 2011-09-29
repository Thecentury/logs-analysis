using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows;
using AdTech.Common.WPF;
using System.Windows.Input;
using LogAnalyzer.Filters;
using System.Collections.ObjectModel;
using LogAnalyzer.GUI.ViewModels;

namespace LogAnalyzer.GUI.ViewModel
{
	public abstract class LogEntriesListViewModel : TabViewModel
	{
		protected LogEntriesListViewModel( ApplicationViewModel applicationViewModel ) : base( applicationViewModel ) { }

		public int TotalEntries
		{
			get { return entries.Count; }
		}

		public int TotalLines
		{
			get
			{
				int count = entries.Sum( le => le.LinesCount );
				return count;
			}
		}

		private SparseLogEntryViewModelList entriesViewModels = null;
		protected SparseLogEntryViewModelList EntriesViewModels
		{
			get { return entriesViewModels; }
		}

		private GenericListView<LogEntryViewModel> entriesView = null;
		public GenericListView<LogEntryViewModel> EntriesView
		{
			get { return entriesView; }
		}

		private IList<LogEntry> entries = null;
		public IList<LogEntry> Entries
		{
			get { return entries; }
		}

		private LogEntryViewModel selectedEntry = null;
		public LogEntryViewModel SelectedEntry
		{
			get { return selectedEntry; }
			set
			{
				if ( selectedEntry == value )
					return;

				selectedEntry = value;
				RaisePropertyChanged( "SelectedEntry" );
			}
		}

		private int selectedEntryIndex = 0;
		public int SelectedEntryIndex
		{
			get { return selectedEntryIndex; }
			set
			{
				if ( selectedEntryIndex == value )
					return;

				selectedEntryIndex = value;
				RaisePropertyChanged( "SelectedEntryIndex" );
			}
		}

		protected internal abstract LogFileViewModel GetFileViewModel( LogEntry logEntry );

		public abstract LogEntriesListViewModel Clone();

		protected void Init( IList<LogEntry> entries )
		{
			if ( entries == null )
				throw new ArgumentNullException( "entries" );

			this.entries = entries;

			this.entriesViewModels = new SparseLogEntryViewModelList( this, GetFileViewModel );
			entriesViewModels.ItemCreated += OnEntriesViewModels_ItemCreated;
			entriesViewModels.ItemRemoved += OnEntriesViewModels_ItemRemoved;

			this.entriesViewModels.CollectionChanged += OnEntriesViewModels_CollectionChanged;

			InvokeInUIDispatcher( () =>
			{
				this.entriesView = new GenericListView<LogEntryViewModel>( entriesViewModels );
			} );
		}

		private void OnEntriesViewModels_ItemCreated( object sender, LogEntryHostChangedEventArgs e )
		{
			OnLogEntryViewModelCreated( e.LogEntryViewModel );
		}

		protected virtual void OnLogEntryViewModelCreated( LogEntryViewModel createdViewModel )
		{
			UpdateDynamicHighlighting();
		}

		private void OnEntriesViewModels_ItemRemoved( object sender, LogEntryHostChangedEventArgs e )
		{
			OnLogEntryViewModelRemoved( e.LogEntryViewModel );
		}

		protected virtual void OnLogEntryViewModelRemoved( LogEntryViewModel removedViewModel )
		{
			UpdateDynamicHighlighting();
		}

		private void OnEntriesViewModels_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			OnEntriesChanged();
		}

		internal virtual void OnEntriesChanged()
		{
			RaisePropertiesChanged( "TotalLines", "TotalEntries" );
		}

		#region Highlighting

		private IFilter<LogEntry> dynamicHighlightingFilter = null;
		public IFilter<LogEntry> DynamicHighlightingFilter
		{
			get { return dynamicHighlightingFilter; }
			set
			{
				if ( dynamicHighlightingFilter == value )
					return;

				dynamicHighlightingFilter = value;
				RaisePropertyChanged( "DynamicHighlightingFilter" );
			}
		}

		private string highlightedPropertyName = null;
		public string HighlightedPropertyName
		{
			get { return highlightedPropertyName; }
			set
			{
				if ( highlightedPropertyName == value )
					return;

				highlightedPropertyName = value;
				RaisePropertyChanged( "HighlightedPropertyName" );
			}
		}

		public void UpdateDynamicHighlighting()
		{
			if ( dynamicHighlightingFilter == null )
				return;
			if ( highlightedPropertyName == null )
				return;

			foreach ( LogEntryViewModel logEntryViewModel in entriesViewModels.CreatedEntries )
			{
				bool include = dynamicHighlightingFilter.Include( logEntryViewModel.LogEntry );

				logEntryViewModel.IsDynamicHighlighted = include;
				logEntryViewModel.HighlightedColumnName = include ? highlightedPropertyName : null;
			}
		}

		private readonly ObservableCollection<HighlightingViewModel> highlightingViewModels = new ObservableCollection<HighlightingViewModel>();
		public ObservableCollection<HighlightingViewModel> HighlightingViewModels
		{
			get { return highlightingViewModels; }
		}

		#endregion

		private DelegateCommand<RoutedEventArgs> gotFocusCommand = null;
		public ICommand GotFocusCommand
		{
			get
			{
				if ( gotFocusCommand == null )
					gotFocusCommand = new DelegateCommand<RoutedEventArgs>( GotFocusExecute );
				return gotFocusCommand;
			}
		}

		public void GotFocusExecute( RoutedEventArgs e )
		{
			HighlightManager.ProcessCellSelection( e );
		}
	}
}
