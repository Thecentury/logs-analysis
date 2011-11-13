using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Windows.Media;
using JetBrains.Annotations;
using LogAnalyzer.Collections;
using LogAnalyzer.Extensions;
using LogAnalyzer.Filters;
using LogAnalyzer.GUI.ViewModels;
using LogAnalyzer.GUI.ViewModels.Collections;

namespace LogAnalyzer.GUI.ViewModels
{
	public sealed class HighlightingViewModel : BindingObject
	{
		private readonly IList<LogEntry> entriesSource;
		private readonly SparseLogEntryViewModelList logEntriesViewModels;
		private readonly IDisposable collectionChangedSubscription;

		internal HighlightingViewModel( [NotNull] IList<LogEntry> entriesSource, [NotNull] SparseLogEntryViewModelList logEntriesViewModels, 
			[NotNull] ExpressionBuilder builder )
		{
			if ( entriesSource == null ) throw new ArgumentNullException( "entriesSource" );
			if ( logEntriesViewModels == null ) throw new ArgumentNullException( "logEntriesViewModels" );
			if (builder == null) throw new ArgumentNullException("builder");

			this.entriesSource = entriesSource;
			this.logEntriesViewModels = logEntriesViewModels;
			logEntriesViewModels.ItemCreated += OnLogEntriesViewModels_ItemCreated;
			this.Filter.ExpressionBuilder = builder;

			INotifyCollectionChanged observableCollection = entriesSource as INotifyCollectionChanged;
			if ( observableCollection != null )
			{
				collectionChangedSubscription = observableCollection.ToObservable()
					.ObserveOn( DispatcherHelper.GetDispatcher() )
					.Select( o => o.EventArgs )
					.Subscribe( OnSourceCollectionChanged );
			}

			observableFilteredEntries = new ReadonlyObservableList<LogEntry>( acceptedEntries );
			FillAcceptedEntries( entriesSource );

			foreach (var logEntryViewModel in logEntriesViewModels.CreatedEntries)
			{
				if ( filter.Include( logEntryViewModel.LogEntry ) )
				{
					logEntryViewModel.HighlightedByList.Add( this );
				}
			}

			filter.Changed += OnFilter_Changed;
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
				}
			}
			else if ( e.Action == NotifyCollectionChangedAction.Reset )
			{
				lock ( acceptedEntriesSync )
				{
					acceptedEntries.Clear();
					FillAcceptedEntries( entriesSource );
					observableFilteredEntries.RaiseCollectionReset();
				}
			}
		}

		private void FillAcceptedEntries( IEnumerable<LogEntry> entries )
		{
			foreach ( var logEntry in entries )
			{
				if ( filter.Include( logEntry ) )
				{
					acceptedEntries.Add( logEntry );
				}
			}
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
		}
	}
}
