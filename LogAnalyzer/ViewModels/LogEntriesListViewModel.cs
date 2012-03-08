using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xaml;
using LogAnalyzer.ColorOverviews;
using LogAnalyzer.Extensions;
using LogAnalyzer.Filters;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.GUI.OverviewGui;
using LogAnalyzer.GUI.Properties;
using LogAnalyzer.GUI.ViewModels.Collections;
using LogAnalyzer.GUI.Extensions;
using LogAnalyzer.Logging;
using LogAnalyzer.Operations;
using Microsoft.Research.DynamicDataDisplay.Common.Palettes;

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
			highlightingFilters.CollectionChanged += OnHighlightingFilters_CollectionChanged;
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
					return;

				_selectedEntry = value;
				RaisePropertyChanged( "SelectedEntry" );
			}
		}

		public int SelectedEntryIndex
		{
			get { return _selectedEntryIndex; }
			set
			{
				_selectedEntryIndex = value;
				RaisePropertyChanged( "SelectedEntryIndex" );

				if ( DataGrid != null )
				{
					//Stopwatch timer = Stopwatch.StartNew();
					var selectedValue = _logEntriesViewModels[value];
					//var elapsed = timer.ElapsedMilliseconds;
					//Logger.Instance.WriteInfo( "Got selectedValue in {0}", elapsed );

					//timer.Restart();
					DataGrid.ScrollIntoView( selectedValue );
					//elapsed = timer.ElapsedMilliseconds;
					//Logger.Instance.WriteInfo( "Scrolled into view in {0}", elapsed );
				}
			}
		}

		public bool AutoScrollToBottom
		{
			get { return _autoScrollToBottom; }
			set
			{
				if ( _autoScrollToBottom == value )
					return;

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
					return;

				_addedEntriesCountString = value;
				RaisePropertyChanged( "AddedEntriesCountString" );
			}
		}

		#region Dynamic highlighting

		private IFilter<LogEntry> dynamicHighlightingFilter;

		private string highlightedPropertyName;

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

			foreach ( LogEntryViewModel logEntryViewModel in _logEntriesViewModels.CreatedEntries )
			{
				bool include = dynamicHighlightingFilter.Include( logEntryViewModel.LogEntry );

				logEntryViewModel.IsDynamicHighlighted = include;
				logEntryViewModel.HighlightedColumnName = include ? highlightedPropertyName : null;
			}
		}

		#endregion

		#region Overviews

		private BitmapSource densityImage;
		public BitmapSource DensityImage
		{
			get
			{
				if ( densityImage == null )
				{
					var collector = new DensityOverviewCollector<LogEntry>();
					var builder = new DensityOverviewBuilder<LogEntry>();
					var grouped = collector.Build( _entries );
					var map = builder.CreateOverviewMap( grouped )
						.Select( e => Math.Pow( e, 0.33 ) )
						.ToArray();

					var bitmapBuilder = new PaletteOverviewBitmapBuilder( new LinearPalette( Colors.White, Colors.DarkBlue ) );
					densityImage = bitmapBuilder.CreateBitmap( map );
				}

				return densityImage;
			}
		}

		private List<OverviewInfo> messageTypeOverviewInfos;
		public List<OverviewInfo> MessageTypeOverview
		{
			get
			{
				if ( messageTypeOverviewInfos == null )
				{
					var collector = new GroupingByIndexOverviewCollector<LogEntry>();
					var builder = new MessageTypeOverviewBuilder();
					var map = builder.CreateOverviewMap( collector.Build( _entries ) );

					double length = map.Length;

					List<OverviewInfo> list = new List<OverviewInfo>();
					for ( int i = 0; i < map.Length; i++ )
					{
						LogEntry entry = map[i];
						if ( entry == null )
							continue;
						if ( !entry.Type.In( MessageTypes.Error, MessageTypes.Warning ) )
							continue;

						OverviewInfo info = new OverviewInfo( map[i], i / length, this );
						list.Add( info );
					}

					messageTypeOverviewInfos = list;
				}

				return messageTypeOverviewInfos;
			}
		}

		private DelegateCommand<LogEntry> scrollToItemCommand;
		public DelegateCommand<LogEntry> ScrollToItemCommand
		{
			get
			{
				if ( scrollToItemCommand == null )
				{
					scrollToItemCommand = new DelegateCommand<LogEntry>( entry =>
					{
						var index = _entries.SequentialIndexOf( entry );
						SelectedEntryIndex = index;
					} );
				}
				return scrollToItemCommand;
			}
		}

		private ObservableCollection<OverviewViewModelBase> overviews;
		public ObservableCollection<OverviewViewModelBase> Overviews
		{
			get
			{
				if ( overviews == null )
				{
					overviews = new ObservableCollection<OverviewViewModelBase>();
					PopulateOverviews( overviews );
				}
				return overviews;
			}
		}

		protected virtual void PopulateOverviews( IList<OverviewViewModelBase> overviewsList )
		{

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
			ScrollToBottomIfShould();
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
			var highlightVM = ApplicationViewModel.ShowHighlightEditorWindow();
			if ( highlightVM == null )
				return;

			var vm = new HighlightingViewModel( this, highlightVM.SelectedBuilder, new SolidColorBrush( highlightVM.SelectedColor ) );

			highlightingFilters.Add( vm );
		}

		// SaveToFile command

		private DelegateCommand saveToFileCommand;
		public ICommand SaveToFileCommand
		{
			get
			{
				if ( saveToFileCommand == null )
				{
					saveToFileCommand = new DelegateCommand( SaveToFileExecute );
				}
				return saveToFileCommand;
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
																using ( FileStream fs = new FileStream( path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None ) )
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

		private readonly ObservableCollection<HighlightingViewModel> highlightingFilters =
			new ObservableCollection<HighlightingViewModel>();

		public ObservableCollection<HighlightingViewModel> HighlightingFilters
		{
			get { return highlightingFilters; }
		}

		private void OnHighlightingFilters_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			if ( e.NewItems != null )
			{
				foreach ( HighlightingViewModel added in e.NewItems )
				{
					added.Changed += OnHighlightingFilter_Changed;
				}
			}
			if ( e.OldItems != null )
			{
				foreach ( HighlightingViewModel removed in e.OldItems )
				{
					removed.Changed -= OnHighlightingFilter_Changed;
					removed.Dispose();
				}
			}
		}

		private void OnHighlightingFilter_Changed( object sender, EventArgs e )
		{
			var filter = (HighlightingViewModel)sender;

			foreach ( LogEntryViewModel entryViewModel in LogEntriesViewModels.CreatedEntries )
			{
				entryViewModel.HighlightedByList.Remove( filter );

				if ( filter.Filter.Include( entryViewModel.LogEntry ) )
				{
					entryViewModel.HighlightedByList.Add( filter );
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
			collection.Add( new ToolBarItemViewModel( "Save entries to file", SaveToFileCommand, MakePackUri( "/Resources/disk.png" ) ) );
			collection.Add( new ToolBarItemViewModel( "Add highlighting", AddHighlightingCommand, MakePackUri( "/Resources/flag--plus.png" ) ) );
		}

		#endregion

		#region StatusBar

		private readonly ObservableCollection<object> statusBarItems = new ObservableCollection<object>();
		private bool statusBarItemsPopulated;

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

			MessageSeverityCountViewModel messagesCount = MessageSeverityCount;
			if ( messagesCount != null )
			{
				collection.Add( messagesCount );
			}

			collection.Add( new SelfWorkingSetStatusBarItem() );
#if DEBUG
			StatusBarLogWriter writer = GetOrCreateLogWriter();
			collection.Add( new LogStatusBarItem( writer ) );
#endif
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
				throw new ArgumentNullException( "entries" );

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