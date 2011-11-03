using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;
using LogAnalyzer.Filters;
using System.Collections.ObjectModel;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.GUI.ViewModels.Collections;

namespace LogAnalyzer.GUI.ViewModels
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

		private SparseLogEntryViewModelList logEntriesViewModels;
		protected SparseLogEntryViewModelList LogEntriesViewModels
		{
			get { return logEntriesViewModels; }
		}

		private GenericListView<LogEntryViewModel> entriesView;
		public GenericListView<LogEntryViewModel> EntriesView
		{
			get { return entriesView; }
		}

		public virtual MessageSeverityCountViewModel MessageSeverityCount
		{
			get { return null; }
		}

		public LogEntriesListViewModel ParentView { get; set; }

		/// <summary>
		/// Есть ли несколько загруженных директорий?
		/// </summary>
		public bool HasSeveralDirectories
		{
			get { return ApplicationViewModel.Core.Directories.Count > 1; }
		}

		/// <summary>
		/// Нужно ли отображать столбец с именем директории?
		/// </summary>
		public Visibility DirectoriesColumnVisibility
		{
			get { return HasSeveralDirectories ? Visibility.Visible : Visibility.Collapsed; }
		}

		private IList<LogEntry> entries;
		public IList<LogEntry> Entries
		{
			get { return entries; }
		}

		private LogEntryViewModel selectedEntry;
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

		private int selectedEntryIndex;
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

		private bool autoScrollToBottom;
		public bool AutoScrollToBottom
		{
			get { return autoScrollToBottom; }
			set
			{
				if ( autoScrollToBottom == value )
					return;

				autoScrollToBottom = value;
				RaisePropertyChanged( "AutoScrollToBottom" );

				if ( autoScrollToBottom )
				{
					ScrollToBottomCommand.Execute( null );
				}
			}
		}

		#region Toolbar

		private bool toolbarItemsPopulated;
		private readonly ObservableCollection<object> toolbarItems = new ObservableCollection<object>();
		public ObservableCollection<object> ToolbarItems
		{
			get
			{
				if ( !toolbarItemsPopulated )
				{
					PopulateToolbarItems( toolbarItems );
					toolbarItemsPopulated = true;
				}

				return toolbarItems;
			}
		}

		protected virtual void PopulateToolbarItems( ICollection<object> collection ) { }

		#endregion

		#region StatusBar

		private bool statusBarItemsPopulated;
		private readonly ObservableCollection<object> statusBarItems = new ObservableCollection<object>();
		public ObservableCollection<object> StatusBarItems
		{
			get
			{
				if ( !statusBarItemsPopulated )
				{
					PopulateStatusBarItems( statusBarItems );
					statusBarItemsPopulated = true;
				}

				return statusBarItems;
			}
		}

		protected virtual void PopulateStatusBarItems( ICollection<object> collection )
		{
			collection.Add( GetEntriesCountStatusBarItem() );
			collection.Add( new SelectedEntryIndexStatusBarItem( this ) );

			var messagesCount = MessageSeverityCount;
			if ( messagesCount != null )
			{
				collection.Add( messagesCount );
			}
		}

		protected virtual EntriesCountStatusBarItem GetEntriesCountStatusBarItem()
		{
			return new EntriesCountStatusBarItem( this );
		}

		#endregion

		protected internal abstract LogFileViewModel GetFileViewModel( LogEntry logEntry );

		public abstract LogEntriesListViewModel Clone();

		protected void Init( IList<LogEntry> entries )
		{
			if ( entries == null )
				throw new ArgumentNullException( "entries" );

			this.entries = entries;

			this.logEntriesViewModels = new SparseLogEntryViewModelList( this, GetFileViewModel );
			logEntriesViewModels.ItemCreated += OnLogEntriesViewModelsItemCreated;
			logEntriesViewModels.ItemRemoved += OnLogEntriesViewModelsItemRemoved;

			this.logEntriesViewModels.CollectionChanged += OnLogEntriesViewModelsCollectionChanged;

			InvokeInUIDispatcher( () =>
			{
				this.entriesView = new GenericListView<LogEntryViewModel>( logEntriesViewModels );
			} );
		}

		private void OnLogEntriesViewModelsItemCreated( object sender, LogEntryHostChangedEventArgs e )
		{
			OnLogEntryViewModelCreated( e.LogEntryViewModel );
		}

		protected virtual void OnLogEntryViewModelCreated( LogEntryViewModel createdViewModel )
		{
			UpdateDynamicHighlighting();
		}

		private void OnLogEntriesViewModelsItemRemoved( object sender, LogEntryHostChangedEventArgs e )
		{
			OnLogEntryViewModelRemoved( e.LogEntryViewModel );
		}

		protected virtual void OnLogEntryViewModelRemoved( LogEntryViewModel removedViewModel )
		{
			UpdateDynamicHighlighting();
		}

		private void OnLogEntriesViewModelsCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
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

		private string highlightedPropertyName;
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

			foreach ( LogEntryViewModel logEntryViewModel in logEntriesViewModels.CreatedEntries )
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

		#region Commands

		private DelegateCommand<RoutedEventArgs> gotFocusCommand;
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
			DynamicHighlightManager.ProcessCellSelection( e );
		}

		#region Scroll commands

		private DelegateCommand scrollDownCommand;
		public ICommand ScrollDownCommand
		{
			get
			{
				if ( scrollDownCommand == null )
					scrollDownCommand = new DelegateCommand( () => SelectedEntryIndex++, () => SelectedEntryIndex < entries.Count - 1 );

				return scrollDownCommand;
			}
		}

		private DelegateCommand scrollUpCommand;
		public ICommand ScrollUpCommand
		{
			get
			{
				if ( scrollUpCommand == null )
					scrollUpCommand = new DelegateCommand( () => SelectedEntryIndex--, () => SelectedEntryIndex > 0 );

				return scrollUpCommand;
			}
		}

		private const int PageSize = 15;

		private DelegateCommand scrollPageDownCommand;
		public ICommand ScrollPageDownCommand
		{
			get
			{
				if ( scrollPageDownCommand == null )
					scrollPageDownCommand = new DelegateCommand(
						() => entriesView.MoveCurrentToPosition( Math.Max( SelectedEntryIndex + PageSize, entries.Count - 1 ) ),
						() => SelectedEntryIndex < entries.Count - 1 );

				return scrollPageDownCommand;
			}
		}

		private DelegateCommand scrollPageUpCommand;
		public ICommand ScrollPageUpCommand
		{
			get
			{
				if ( scrollPageUpCommand == null )
					scrollPageUpCommand = new DelegateCommand(
						() => SelectedEntryIndex = Math.Max( 0, SelectedEntryIndex - PageSize ),
						() => SelectedEntryIndex > 0 );

				return scrollPageUpCommand;
			}
		}

		private DelegateCommand scrollToTopCommand;
		public ICommand ScrollToTopCommand
		{
			get
			{
				if ( scrollToTopCommand == null )
					scrollToTopCommand = new DelegateCommand( () => SelectedEntryIndex = 0 );

				return scrollToTopCommand;
			}
		}

		private DelegateCommand scrollToBottomCommand;
		public ICommand ScrollToBottomCommand
		{
			get
			{
				if ( scrollToBottomCommand == null )
					scrollToBottomCommand = new DelegateCommand( () => SelectedEntryIndex = entries.Count - 1 );

				return scrollToBottomCommand;
			}
		}

		#endregion

		#endregion
	}
}
