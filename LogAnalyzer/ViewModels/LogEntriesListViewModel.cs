using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using LogAnalyzer.ColorOverviews;
using LogAnalyzer.Extensions;
using LogAnalyzer.Filters;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.GUI.OverviewGui;
using LogAnalyzer.GUI.Properties;
using LogAnalyzer.GUI.ViewModels.Collections;
using LogAnalyzer.GUI.Extensions;
using LogAnalyzer.Logging;
using Microsoft.Research.DynamicDataDisplay.Common.Palettes;

namespace LogAnalyzer.GUI.ViewModels
{
	public abstract class LogEntriesListViewModel : TabViewModel
	{
		private int addedEntriesCount;

		private string addedEntriesCountString;
		private bool autoScrollToBottom = true;
		private IList<LogEntry> entries;
		private LogEntryViewModelCollectionView entriesView;
		private SparseLogEntryViewModelList logEntriesViewModels;
		private LogEntryViewModel selectedEntry;
		private int selectedEntryIndex;
		private DispatcherTimer updateAddedCountTimer;
		private SearchViewModel searchViewModel;

		protected LogEntriesListViewModel( ApplicationViewModel applicationViewModel )
			: base( applicationViewModel )
		{
			highlightingFilters.CollectionChanged += OnHighlightingFilters_CollectionChanged;
		}

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

		protected internal SparseLogEntryViewModelList LogEntriesViewModels
		{
			get { return logEntriesViewModels; }
		}

		public LogEntryViewModelCollectionView EntriesView
		{
			get { return entriesView; }
		}

		public SearchViewModel SearchViewModel
		{
			get { return searchViewModel; }
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
			get { return entries; }
		}

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

		public int SelectedEntryIndex
		{
			get { return selectedEntryIndex; }
			set
			{
				selectedEntryIndex = value;
				RaisePropertyChanged( "SelectedEntryIndex" );

				if ( DataGrid != null )
				{
					//Stopwatch timer = Stopwatch.StartNew();
					var selectedValue = logEntriesViewModels[value];
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
			get { return autoScrollToBottom; }
			set
			{
				if ( autoScrollToBottom == value )
					return;

				autoScrollToBottom = value;
				RaisePropertyChanged( "AutoScrollToBottom" );

				ScrollDownIfShould();
			}
		}

		public string AddedEntriesCountString
		{
			get { return addedEntriesCountString; }
			set
			{
				if ( addedEntriesCountString == value )
					return;

				addedEntriesCountString = value;
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

			foreach ( LogEntryViewModel logEntryViewModel in logEntriesViewModels.CreatedEntries )
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
					var collector = new DensityOverviewCollector<LogEntry>( e => true );
					var builder = new DensityOverviewBuilder<LogEntry>();
					var map = builder.CreateOverviewMap( collector.Build( entries ) )
						.Select( e => Math.Pow( e, 0.33 ) )
						.ToArray();

					var bitmapBuilder = new PaletteOverviewBitmapBuilder( new LinearPalette( Colors.White, Colors.DarkBlue ) );
					densityImage = bitmapBuilder.CreateBitmap( map );
				}

				return densityImage;
			}
		}

		private BitmapSource messageTypeImage;
		public BitmapSource MessageTypeImage
		{
			get
			{
				if ( messageTypeImage == null )
				{
					var collector = new GroupingOverviewCollector<LogEntry>( e => true );
					var builder = new MessageTypeOverviewBuilder();
					var map = builder.CreateOverviewMap( collector.Build( entries ) );

					var bitmapBuilder = new MessageTypeBitmapBuilder();
					messageTypeImage = bitmapBuilder.CreateBitmap( map );
				}
				return messageTypeImage;
			}
		}

		#endregion Overviews

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

		protected override void OnLoaded()
		{
			base.OnLoaded();
			ScrollDownIfShould();
		}

		#region Scroll commands

		private DelegateCommand scrollDownCommand;
		private DelegateCommand scrollPageDownCommand;
		private DelegateCommand scrollPageUpCommand;
		private DelegateCommand scrollToBottomCommand;
		private DelegateCommand scrollToTopCommand;

		private DelegateCommand scrollUpCommand;
		private DelegateCommand toggleAutoScrollToBottomCommand;

		public ScrollViewer ScrollViewer { get; set; }

		public DataGrid DataGrid { get; set; }

		public ICommand ScrollDownCommand
		{
			get
			{
				if ( scrollDownCommand == null )
					scrollDownCommand = new DelegateCommand( () => SelectedEntryIndex++, () => SelectedEntryIndex < entries.Count - 1 );

				return scrollDownCommand;
			}
		}

		public ICommand ScrollUpCommand
		{
			get
			{
				if ( scrollUpCommand == null )
					scrollUpCommand = new DelegateCommand( () => SelectedEntryIndex--, () => SelectedEntryIndex > 0 );

				return scrollUpCommand;
			}
		}

		public ICommand ScrollPageDownCommand
		{
			get
			{
				if ( scrollPageDownCommand == null )
					scrollPageDownCommand = new DelegateCommand(
						() => ScrollViewer.PageDown(),
						() => SelectedEntryIndex < entries.Count - 1 );

				return scrollPageDownCommand;
			}
		}

		public ICommand ScrollPageUpCommand
		{
			get
			{
				if ( scrollPageUpCommand == null )
					scrollPageUpCommand = new DelegateCommand(
						() => ScrollViewer.PageUp(),
						() => SelectedEntryIndex > 0 );

				return scrollPageUpCommand;
			}
		}

		public ICommand ScrollToTopCommand
		{
			get
			{
				if ( scrollToTopCommand == null )
					scrollToTopCommand = new DelegateCommand( () => ScrollViewer.ScrollToTop(), () => ScrollViewer != null );

				return scrollToTopCommand;
			}
		}

		public ICommand ScrollToBottomCommand
		{
			get
			{
				if ( scrollToBottomCommand == null )
					scrollToBottomCommand = new DelegateCommand( () => ScrollViewer.ScrollToBottom(), () => ScrollViewer != null );

				return scrollToBottomCommand;
			}
		}

		public ICommand ToggleAutoScrollToBottomCommand
		{
			get
			{
				if ( toggleAutoScrollToBottomCommand == null )
					toggleAutoScrollToBottomCommand = new DelegateCommand( () => AutoScrollToBottom = !AutoScrollToBottom );
				return toggleAutoScrollToBottomCommand;
			}
		}

		#endregion // Scroll commands

		private DelegateCommand addHighlightingCommand;
		public ICommand AddHighlightingCommand
		{
			get
			{
				if ( addHighlightingCommand == null )
					addHighlightingCommand = new DelegateCommand( AddHighlighting );

				return addHighlightingCommand;
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

		private readonly ObservableCollection<object> toolbarItems = new ObservableCollection<object>();
		private bool toolbarItemsPopulated;

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

		protected virtual void PopulateToolbarItems( ICollection<object> collection )
		{
			collection.Add( new LogEntryListToolbarViewModel( this ) );
			collection.Add( new ToolBarItemViewModel
								{
									Tooltip = "Add highlighting",
									Command = AddHighlightingCommand,
									IconSource = MakePackUri( "/Resources/flag--plus.png" )
								} );
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

			this.entries = entries;

			logEntriesViewModels = new SparseLogEntryViewModelList( this, GetFileViewModel );
			logEntriesViewModels.ItemCreated += OnLogEntriesViewModelsItemCreated;
			logEntriesViewModels.ItemRemoved += OnLogEntriesViewModelsItemRemoved;
			logEntriesViewModels.CollectionChanged += OnLogEntriesViewModelsCollectionChanged;

			searchViewModel = new SearchViewModel( this );

			InvokeInUIDispatcher( () =>
			{
				entriesView = new LogEntryViewModelCollectionView( logEntriesViewModels );
				updateAddedCountTimer = new DispatcherTimer
				{
					Interval = TimeSpan.FromSeconds( Settings.Default.AddedCountUpdateInterval )
				};
				updateAddedCountTimer.Tick += OnUpdateAddedCountTimer_Tick;
				updateAddedCountTimer.Start();
			} );
		}

		protected override void OnTabClosing()
		{
			base.OnTabClosing();

			logEntriesViewModels.Dispose();
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
#if DEBUG
			//var dispatcher = DispatcherHelper.GetDispatcher();
			//if ( dispatcher != null )
			//{
			//    dispatcher.VerifyAccess();
			//}
#endif
			Logger.Instance.WriteError( "{0}: {1} +{2} starting from {3}", GetType().Name,
				e.Action, e.NewItems != null ? e.NewItems.Count : 0, e.NewStartingIndex );

			ScrollDownIfShould();

			RaisePropertiesChanged( "TotalLines", "TotalEntries" );

			UpdateAddedCount( e );
		}

		private void ScrollDownIfShould()
		{
			if ( autoScrollToBottom )
			{
				Dispatcher.CurrentDispatcher.BeginInvoke( ScrollToBottomCommand.ExecuteIfCan );
			}
		}

		private void OnUpdateAddedCountTimer_Tick( object sender, EventArgs e )
		{
			AddedEntriesCountString = "+" + addedEntriesCount;
			addedEntriesCount = 0;
		}

		private void UpdateAddedCount( NotifyCollectionChangedEventArgs e )
		{
			if ( e.Action == NotifyCollectionChangedAction.Add )
			{
				addedEntriesCount += e.NewItems.Count;
			}
			else if ( e.Action == NotifyCollectionChangedAction.Reset )
			{
				addedEntriesCount = 0;
			}
		}
	}
}