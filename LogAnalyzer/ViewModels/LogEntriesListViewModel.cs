using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using LogAnalyzer.ColorOverviews;
using LogAnalyzer.Extensions;
using LogAnalyzer.Filters;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.GUI.OverviewGui;
using LogAnalyzer.GUI.Properties;
using LogAnalyzer.GUI.ViewModels.Collections;
using LogAnalyzer.GUI.Extensions;
using LogAnalyzer.Kernel.Notifications;
using LogAnalyzer.Logging;
using LogAnalyzer.Operations;
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay.DataSources;

namespace LogAnalyzer.GUI.ViewModels
{
	public abstract class LogEntriesListViewModel : TabViewModel
	{
		private int _addedEntriesCount;

		private string _addedEntriesCountString;
		private bool _autoScrollToBottom;
		private IList<LogEntry> _entries;
		private LogEntryViewModelCollectionView _entriesView;
		private SparseLogEntryViewModelList _logEntriesViewModels;
		private LogEntryViewModel _selectedEntry;
		private int _selectedEntryIndex;
		private DispatcherTimer _updateAddedCountTimer;
		private SearchViewModel _searchViewModel;

		protected LogEntriesListViewModel( ApplicationViewModel applicationViewModel )
			: base( applicationViewModel )
		{
			_highlightingFilters.CollectionChanged += OnHighlightingFiltersCollectionChanged;
		}

		public int TotalEntries
		{
			get { return _entries.Count; }
		}

		public int TotalLines
		{
			get
			{
				int count = _entries.Sum( le => le.LinesCount );
				return count;
			}
		}

		protected internal SparseLogEntryViewModelList LogEntriesViewModels
		{
			get { return _logEntriesViewModels; }
		}

		public LogEntryViewModelCollectionView EntriesView
		{
			get { return _entriesView; }
		}

		public SearchViewModel SearchViewModel
		{
			get { return _searchViewModel; }
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

		public IList<LogEntry> Entries
		{
			get { return _entries; }
		}

		public LogEntryViewModel SelectedEntry
		{
			get { return _selectedEntry; }
			set
			{
				if ( _selectedEntry == value )
				{
					return;
				}

				_selectedEntry = value;

				UpdateTimeDelta();

				RaisePropertyChanged( "SelectedEntry" );
			}
		}

		private void UpdateTimeDelta()
		{
			var selectedTime = DateTime.MinValue;
			if ( SelectedEntry != null )
			{
				selectedTime = SelectedEntry.Time;
			}
			foreach ( var entry in _logEntriesViewModels.CreatedEntries )
			{
				if ( entry == SelectedEntry || SelectedEntry == null )
				{
					entry.TimeDelta = null;
				}
				else
				{
					var delta = entry.Time - selectedTime;
					string format;
					if ( delta.Days == 0 )
					{
						if ( delta.Hours == 0 )
						{
							if ( delta.Minutes == 0 )
							{
								format = @"s\s";
							}
							else
							{
								format = @"m\m\ s\s";
							}
						}
						else
						{
							format = @"h\h\ m\m";
						}
					}
					else
					{
						format = @"d\d\ h\h";
					}

					if ( delta.TotalDays < 0 )
					{
						format = @"\-" + format;
					}
					entry.TimeDelta = delta.ToString( format );
				}
			}
		}

		public int SelectedEntryIndex
		{
			get { return _selectedEntryIndex; }
			set
			{
				if ( _selectedEntryIndex == value )
				{
					return;
				}

				_selectedEntryIndex = value;

				if ( DataGrid != null )
				{
					if ( value < _logEntriesViewModels.Count )
					{
						var selectedValue = _logEntriesViewModels[value];
						try
						{
							DataGrid.ScrollIntoView( selectedValue );
						}
						catch ( Exception exc )
						{
							Logger.Instance.WriteException( exc );
						}
					}
				}

				RaisePropertyChanged( "SelectedEntryIndex" );
			}
		}

		public bool AutoScrollToBottom
		{
			get { return _autoScrollToBottom; }
			set
			{
				if ( _autoScrollToBottom == value )
				{
					return;
				}

				_autoScrollToBottom = value;
				RaisePropertyChanged( "AutoScrollToBottom" );

				ScrollToBottomIfShould();
			}
		}

		public string AddedEntriesCountString
		{
			get { return _addedEntriesCountString; }
			set
			{
				if ( _addedEntriesCountString == value )
				{
					return;
				}

				_addedEntriesCountString = value;
				RaisePropertyChanged( "AddedEntriesCountString" );
			}
		}

		#region Dynamic highlighting

		private IFilter<LogEntry> _dynamicHighlightingFilter;

		private string _highlightedPropertyName;

		public IFilter<LogEntry> DynamicHighlightingFilter
		{
			get { return _dynamicHighlightingFilter; }
			set
			{
				if ( _dynamicHighlightingFilter == value )
					return;

				_dynamicHighlightingFilter = value;
				RaisePropertyChanged( "DynamicHighlightingFilter" );
			}
		}

		public string HighlightedPropertyName
		{
			get { return _highlightedPropertyName; }
			set
			{
				if ( _highlightedPropertyName == value )
					return;

				_highlightedPropertyName = value;
				RaisePropertyChanged( "HighlightedPropertyName" );
			}
		}

		public void UpdateDynamicHighlighting()
		{
			if ( _dynamicHighlightingFilter == null )
				return;
			if ( _highlightedPropertyName == null )
				return;

			foreach ( LogEntryViewModel logEntryViewModel in _logEntriesViewModels.CreatedEntries )
			{
				bool include = _dynamicHighlightingFilter.Include( logEntryViewModel.LogEntry );

				logEntryViewModel.IsDynamicHighlighted = include;
				logEntryViewModel.HighlightedColumnName = include ? _highlightedPropertyName : null;
			}
		}

		#endregion

		#region Overviews

		private IPointDataSource _messagesDensityDataSource;
		public IPointDataSource MessagesDensityDataSource
		{
			get
			{
				if ( _messagesDensityDataSource == null )
				{
					Task.Factory.StartNew( () =>
											{
												var collector = new DensityOverviewCollector<LogEntry>();
												var grouped = collector.Build( _entries );

												EnumerableDataSource<IndexedLogEntry> dataSource = new EnumerableDataSource<IndexedLogEntry>(
													grouped.Select( collection => new IndexedLogEntry { Count = collection.Item.Count, Time = collection.Time } ) );

												dataSource.SetXMapping( item => item.Time.Ticks / 10000000000.0 );
												dataSource.SetYMapping( item => item.Count );

												MessagesDensityDataSource = dataSource;
											} );
				}

				return _messagesDensityDataSource;
			}
			private set
			{
				_messagesDensityDataSource = value;
				RaisePropertyChanged( "MessagesDensityDataSource" );
			}
		}

		private struct IndexedLogEntry
		{
			public int Count { get; set; }
			public DateTime Time { get; set; }
		}

		private ObservableCollection<IOverviewViewModel> _overviews;
		public ObservableCollection<IOverviewViewModel> Overviews
		{
			get
			{
				if ( _overviews == null )
				{
					_overviews = new ObservableCollection<IOverviewViewModel>();
					PopulateOverviews( _overviews );
				}
				return _overviews;
			}
		}

		protected virtual void PopulateOverviews( IList<IOverviewViewModel> overviewsList )
		{
			overviewsList.Add( new MessageTypeOverview( _entries, this ) );
			AddLogFileOverview( overviewsList );
			overviewsList.Add( new ThreadOverview( _entries, this ) );
		}

		protected virtual void AddLogFileOverview( IList<IOverviewViewModel> overviewsList )
		{
			overviewsList.Add( new LogFileOverview( _entries, this ) );
		}

		private DelegateCommand<LogEntry> _scrollToItemCommand;
		public DelegateCommand<LogEntry> ScrollToItemCommand
		{
			get
			{
				if ( _scrollToItemCommand == null )
				{
					_scrollToItemCommand = new DelegateCommand<LogEntry>( entry =>
					{
						var index = _entries.SequentialIndexOf( entry );
						SelectedEntryIndex = index;
					} );
				}
				return _scrollToItemCommand;
			}
		}

		#endregion Overviews

		#region Commands

		private DelegateCommand<RoutedEventArgs> _gotFocusCommand;

		public ICommand GotFocusCommand
		{
			get
			{
				if ( _gotFocusCommand == null )
					_gotFocusCommand = new DelegateCommand<RoutedEventArgs>( GotFocusExecute );

				return _gotFocusCommand;
			}
		}

		public void GotFocusExecute( RoutedEventArgs e )
		{
			DynamicHighlightManager.ProcessCellSelection( e );
		}

		protected override void OnLoaded( RoutedEventArgs e )
		{
			base.OnLoaded( e );
			ScrollToBottomCommand.ExecuteIfCan();
		}

		#region Scroll commands

		private DelegateCommand _scrollDownCommand;
		private DelegateCommand _scrollPageDownCommand;
		private DelegateCommand _scrollPageUpCommand;
		private DelegateCommand _scrollToBottomCommand;
		private DelegateCommand _scrollToTopCommand;

		private DelegateCommand _scrollUpCommand;
		private DelegateCommand _toggleAutoScrollToBottomCommand;

		public ScrollViewer ScrollViewer { get; set; }

		public DataGrid DataGrid { get; set; }

		public ICommand ScrollDownCommand
		{
			get
			{
				if ( _scrollDownCommand == null )
					_scrollDownCommand = new DelegateCommand( () => SelectedEntryIndex++, () => SelectedEntryIndex < _entries.Count - 1 );

				return _scrollDownCommand;
			}
		}

		public ICommand ScrollUpCommand
		{
			get
			{
				if ( _scrollUpCommand == null )
					_scrollUpCommand = new DelegateCommand( () => SelectedEntryIndex--, () => SelectedEntryIndex > 0 );

				return _scrollUpCommand;
			}
		}

		public ICommand ScrollPageDownCommand
		{
			get
			{
				if ( _scrollPageDownCommand == null )
					_scrollPageDownCommand = new DelegateCommand(
						() => ScrollViewer.PageDown(),
						() => SelectedEntryIndex < _entries.Count - 1 );

				return _scrollPageDownCommand;
			}
		}

		public ICommand ScrollPageUpCommand
		{
			get
			{
				if ( _scrollPageUpCommand == null )
					_scrollPageUpCommand = new DelegateCommand(
						() => ScrollViewer.PageUp(),
						() => SelectedEntryIndex > 0 );

				return _scrollPageUpCommand;
			}
		}

		public ICommand ScrollToTopCommand
		{
			get
			{
				if ( _scrollToTopCommand == null )
					_scrollToTopCommand = new DelegateCommand( () => ScrollViewer.ScrollToTop(), () => ScrollViewer != null );

				return _scrollToTopCommand;
			}
		}

		public ICommand ScrollToBottomCommand
		{
			get
			{
				if ( _scrollToBottomCommand == null )
					_scrollToBottomCommand = new DelegateCommand( () => ScrollViewer.ScrollToBottom(), () => ScrollViewer != null );

				return _scrollToBottomCommand;
			}
		}

		public ICommand ToggleAutoScrollToBottomCommand
		{
			get
			{
				if ( _toggleAutoScrollToBottomCommand == null )
					_toggleAutoScrollToBottomCommand = new DelegateCommand( () => AutoScrollToBottom = !AutoScrollToBottom );
				return _toggleAutoScrollToBottomCommand;
			}
		}

		#endregion // Scroll commands

		private DelegateCommand _addHighlightingCommand;
		public ICommand AddHighlightingCommand
		{
			get
			{
				if ( _addHighlightingCommand == null )
					_addHighlightingCommand = new DelegateCommand( AddHighlighting );

				return _addHighlightingCommand;
			}
		}

		private void AddHighlighting()
		{
			var highlightVm = ApplicationViewModel.ShowHighlightEditorWindow();
			if ( highlightVm == null )
				return;

			var vm = new HighlightingViewModel( this, highlightVm.SelectedBuilder, new SolidColorBrush( highlightVm.SelectedColor ).AsFrozen() );

			_highlightingFilters.Add( vm );
		}

		// Add filter tab command

		private DelegateCommand _addFilterTabCommand;
		public ICommand AddFilterTabCommand
		{
			get
			{
				if ( _addFilterTabCommand == null )
				{
					_addFilterTabCommand = new DelegateCommand( AddFilterTabExecute );
				}
				return _addFilterTabCommand;
			}
		}

		private void AddFilterTabExecute()
		{
			ExpressionBuilder filterBuilder = ApplicationViewModel.ShowFilterEditorWindow();
			if ( filterBuilder == null )
			{
				return;
			}

			FilterTabViewModel filterTab = new FilterTabViewModel( _entries, ApplicationViewModel );
			filterTab.Filter.ExpressionBuilder = filterBuilder;

			ApplicationViewModel.Tabs.Add( filterTab );
			filterTab.StartFiltering();

		}

		// SaveToFile command

		private DelegateCommand _saveToFileCommand;
		public ICommand SaveToFileCommand
		{
			get
			{
				if ( _saveToFileCommand == null )
				{
					_saveToFileCommand = new DelegateCommand( SaveToFileExecute );
				}
				return _saveToFileCommand;
			}
		}

		private void SaveToFileExecute()
		{
			var saveFileDialog = ApplicationViewModel.Config.ResolveNotNull<ISaveFileDialog>();
			if ( saveFileDialog.ShowDialog() == true )
			{
				string path = saveFileDialog.FileName;

				OperationScheduler operationScheduler = ApplicationViewModel.Config.ResolveNotNull<OperationScheduler>();

				operationScheduler.StartNewOperation( () =>
														{
															try
															{
																using ( FileStream fs = new FileStream( path, FileMode.Create, FileAccess.Write, FileShare.None ) )
																using ( var writer = new StreamWriter( fs ) )
																{
																	LogSaveVisitor saveVisitor = new LogSaveVisitor( writer, new DefaultLogEntryFormatter() );
																	foreach ( var logEntry in _entries )
																	{
																		logEntry.Accept( saveVisitor );
																	}
																}
															}
															catch ( Exception exc )
															{
																Logger.Instance.WriteError( "SaveToFileExecute(): {0}", exc );
															}
														} );
			}
		}

		#endregion // commands

		#region Highlighting

		private readonly ObservableCollection<HighlightingViewModel> _highlightingFilters =
			new ObservableCollection<HighlightingViewModel>();

		public ObservableCollection<HighlightingViewModel> HighlightingFilters
		{
			get { return _highlightingFilters; }
		}

		private void OnHighlightingFiltersCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			if ( e.NewItems != null )
			{
				foreach ( HighlightingViewModel added in e.NewItems )
				{
					added.Changed += OnHighlightingFilterChanged;
				}
			}
			if ( e.OldItems != null )
			{
				foreach ( HighlightingViewModel removed in e.OldItems )
				{
					removed.Changed -= OnHighlightingFilterChanged;
					removed.Dispose();
				}
			}
		}

		private void OnHighlightingFilterChanged( object sender, EventArgs e )
		{
			var highlighter = (HighlightingViewModel)sender;

			foreach ( LogEntryViewModel entryViewModel in LogEntriesViewModels.CreatedEntries )
			{
				entryViewModel.HighlightedByList.Remove( highlighter );

				if ( highlighter.Filter.Include( entryViewModel.LogEntry ) )
				{
					entryViewModel.HighlightedByList.Add( highlighter );
				}
			}
		}

		#endregion

		#region Toolbar

		private readonly ObservableCollection<object> _toolbarItems = new ObservableCollection<object>();
		private bool _toolbarItemsPopulated;

		public ObservableCollection<object> ToolbarItems
		{
			get
			{
				if ( !_toolbarItemsPopulated )
				{
					PopulateToolbarItems( _toolbarItems );
					_toolbarItemsPopulated = true;
				}

				return _toolbarItems;
			}
		}

		protected virtual void PopulateToolbarItems( IList<object> collection )
		{
			collection.Add( new LogEntryListToolbarViewModel( this ) );
			collection.Add( new ToolBarItemViewModel( "Add filter tab", AddFilterTabCommand, MakePackUri( "/Resources/funnel--plus.png" ) ) );
			collection.Add( new ToolBarItemViewModel( "Save entries to file", SaveToFileCommand, MakePackUri( "/Resources/disk.png" ) ) );
			collection.Add( new ToolBarItemViewModel( "Add highlighting", AddHighlightingCommand, MakePackUri( "/Resources/flag--plus.png" ) ) );

			var notificationSource = GetNotificationSource();
			if ( notificationSource != null )
			{
				collection.Add( new ToolBarItemViewModel( "Force update", CreateForceUpdateCommand( notificationSource ), MakePackUri( "/Resources/arrow-retweet.png" ) ) );
			}
		}

		protected virtual LogNotificationsSourceBase GetNotificationSource()
		{
			return null;
		}

		private ICommand CreateForceUpdateCommand( LogNotificationsSourceBase notificationsSource )
		{
			return new DelegateCommand( () => notificationsSource.ForceUpdate() );
		}

		#endregion

		#region StatusBar

		private readonly ObservableCollection<object> _statusBarItems = new ObservableCollection<object>();
		private bool _statusBarItemsPopulated;

		public ObservableCollection<object> StatusBarItems
		{
			get
			{
				if ( !_statusBarItemsPopulated )
				{
					PopulateStatusBarItems( _statusBarItems );
					_statusBarItemsPopulated = true;
				}

				return _statusBarItems;
			}
		}

		protected virtual void PopulateStatusBarItems( ICollection<object> collection )
		{
			collection.Add( GetEntriesCountStatusBarItem() );
			collection.Add( new SelectedEntryIndexStatusBarItem( this ) );

			MessageSeverityCountViewModel messagesCount = MessageSeverityCount;
			if ( messagesCount != null )
			{
				collection.Add( messagesCount );
			}

			collection.Add( new SelfWorkingSetStatusBarItem() );

			StatusBarLogWriter writer = GetOrCreateLogWriter();
			collection.Add( new LogStatusBarItem( writer ) );
		}

		private StatusBarLogWriter GetOrCreateLogWriter()
		{
			StatusBarLogWriter writer = Logger.Instance.Writers.OfType<StatusBarLogWriter>().FirstOrDefault();
			if ( writer == null )
			{
				writer = new StatusBarLogWriter();
				Logger.Instance.Writers.Add( writer );
			}

			return writer;
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
			{
				throw new ArgumentNullException( "entries" );
			}

			this._entries = entries;

			_logEntriesViewModels = new SparseLogEntryViewModelList( this, GetFileViewModel );
			_logEntriesViewModels.ItemCreated += OnLogEntriesViewModelsItemCreated;
			_logEntriesViewModels.ItemRemoved += OnLogEntriesViewModelsItemRemoved;
			_logEntriesViewModels.CollectionChanged += OnLogEntriesViewModelsCollectionChanged;

			_searchViewModel = new SearchViewModel( this );

			InvokeInUIDispatcher( () =>
			{
				_entriesView = new LogEntryViewModelCollectionView( _logEntriesViewModels );

				_updateAddedCountTimer = new DispatcherTimer
				{
					Interval = TimeSpan.FromSeconds( Settings.Default.AddedCountUpdateInterval )
				};
				_updateAddedCountTimer.Tick += OnUpdateAddedCountTimerTick;
				_updateAddedCountTimer.Start();
			} );
		}

		protected override void OnTabClosing()
		{
			base.OnTabClosing();

			_logEntriesViewModels.Dispose();
		}

		private void OnLogEntriesViewModelsItemCreated( object sender, LogEntryHostChangedEventArgs e )
		{
			OnLogEntryViewModelCreated( e.LogEntryViewModel );
		}

		protected virtual void OnLogEntryViewModelCreated( LogEntryViewModel createdViewModel )
		{
			UpdateDynamicHighlighting();
			UpdateTimeDelta();
		}

		private void OnLogEntriesViewModelsItemRemoved( object sender, LogEntryHostChangedEventArgs e )
		{
			OnLogEntryViewModelRemoved( e.LogEntryViewModel );
		}

		protected virtual void OnLogEntryViewModelRemoved( LogEntryViewModel removedViewModel )
		{
			UpdateDynamicHighlighting();
			UpdateTimeDelta();
		}

		private void OnLogEntriesViewModelsCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			OnLogEntriesViewModelsCollectionChanged( e );
		}

		/// <summary>
		/// <remarks>Вызывается в UI потоке.</remarks>
		/// </summary>
		/// <param name="e"></param>
		protected virtual void OnLogEntriesViewModelsCollectionChanged( NotifyCollectionChangedEventArgs e )
		{
			ScrollToBottomIfShould();

			RaisePropertiesChanged( "TotalLines", "TotalEntries" );

			UpdateAddedCount( e );
		}

		private void ScrollToBottomIfShould()
		{
			if ( _autoScrollToBottom )
			{
				ScrollToBottomCommand.ExecuteIfCan();
			}
		}

		private void OnUpdateAddedCountTimerTick( object sender, EventArgs e )
		{
			AddedEntriesCountString = "+" + _addedEntriesCount;
			_addedEntriesCount = 0;
		}

		private void UpdateAddedCount( NotifyCollectionChangedEventArgs e )
		{
			if ( e.Action == NotifyCollectionChangedAction.Add )
			{
				_addedEntriesCount += e.NewItems.Count;
			}
			else if ( e.Action == NotifyCollectionChangedAction.Reset )
			{
				_addedEntriesCount = 0;
			}
		}

		public void OnSelectedTextChanged( string text )
		{
			if ( String.IsNullOrWhiteSpace( text ) )
				return;

			Regex textSearchRegex = CreateTextSearchRegex( text );
			foreach ( var logEntryViewModel in LogEntriesViewModels.CreatedEntries )
			{
				var document = logEntryViewModel.Document;
				if ( document == null )
					return;

				//string xaml = XamlServices.Save( document );
				//Logger.Instance.WriteInfo( xaml );

				string unitedText = logEntryViewModel.UnitedText;
				var match = textSearchRegex.Match( unitedText );

				var start = document.ContentStart;

				while ( match.Success )
				{
					var selectionRange = new TextRange( start.GetPositionAtOffset( match.Index ),
													   start.GetPositionAtOffset( match.Index + match.Length ) );

					selectionRange.ApplyPropertyValue( TextElement.BackgroundProperty, Brushes.LightGreen );

					match = match.NextMatch();
				}
			}
		}

		private Regex CreateTextSearchRegex( string text )
		{
			string escapedText = text.Escape( "[", "]", "(", ")", @"\", ".", "+", "*", "?" );

			Regex regex = new Regex( escapedText, RegexOptions.Compiled | RegexOptions.Multiline );
			return regex;
		}
	}
}