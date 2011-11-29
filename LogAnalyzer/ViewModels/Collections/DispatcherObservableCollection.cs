using System;
using System.Collections.Specialized;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using LogAnalyzer.Collections;
using LogAnalyzer.Extensions;
using System.ComponentModel;
using LogAnalyzer.Logging;

namespace LogAnalyzer.GUI.ViewModels.Collections
{
	/// <summary>
	/// Передает событие CollectionChanged в UI поток.
	/// </summary>
	[IgnoreMissingProperty( "Count" )]
	public class DispatcherObservableCollection : INotifyCollectionChanged, INotifyPropertyChanged, IDisposable
	{
		private readonly IScheduler scheduler;
		private readonly object collection;
		private readonly CompositeDisposable unsubscruber;

		public DispatcherObservableCollection( object collection, IScheduler scheduler )
		{
			if ( collection == null ) throw new ArgumentNullException( "collection" );
			if ( scheduler == null ) throw new ArgumentNullException( "scheduler" );

			this.collection = collection;
			this.scheduler = scheduler;

			INotifyCollectionChanged observableCollection = (INotifyCollectionChanged)collection;

			// todo brinchuk remove me
			observableCollection.CollectionChanged += observableCollection_CollectionChanged;

			var observable = Observable.FromEventPattern<NotifyCollectionChangedEventArgs>( observableCollection,
																						   "CollectionChanged" );

			var unsubscribeAdd = observable.Where( e => e.EventArgs.Action == NotifyCollectionChangedAction.Add )
				.Buffer( TimeSpan.FromSeconds( 1 ) )
				.ObserveOn( scheduler )
				.SelectMany( e => e )
				.Subscribe( e => OnAdded( e.EventArgs ) );

			var collectionChanged = observable
				.ObserveOn( scheduler );
			var unsubscribeOthers = collectionChanged.Where( e => e.EventArgs.Action != NotifyCollectionChangedAction.Add )
				.Subscribe( e => OnCollectionChanged( e.EventArgs ) );

			unsubscriber = new CompositeDisposable( unsubscribeAdd, unsubscribeOthers );
		}

		// todo brinchuk remove me!!!
		void observableCollection_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			if ( e.Action != NotifyCollectionChangedAction.Add )
				return;

			var compositeList = collection as CompositeObservableListWrapper<LogEntry>;
			if ( compositeList == null )
				return;


			for ( int i = 0; i < e.NewItems.Count; i++ )
			{
				int index = e.NewStartingIndex + i;

				var entry = (LogEntry)e.NewItems[i];
				int adjustedIndex = ParallelHelper.SequentialIndexOf( compositeList, entry, 0 );

				if ( adjustedIndex != index )
				{
					Logger.Instance.WriteError( "Adjusted index: was {0}, now {1}", index, adjustedIndex );
				}
			}
		}

		private void OnAdded( NotifyCollectionChangedEventArgs e )
		{
			int index = e.NewStartingIndex;

			for ( int i = 0; i < e.NewItems.Count; i++ )
			{
				var addedItem = e.NewItems[i];
				OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, addedItem, index + i ) );
			}
		}

		protected virtual void OnCollectionChanged( NotifyCollectionChangedEventArgs e )
		{
			CollectionChanged.RaiseCollectionChanged( this, e );
			PropertyChanged.Raise( this, "Count" );
		}

		#region INotifyCollectionChanged Members

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public void RaiseCollectionReset()
		{
			scheduler.Schedule( CollectionChanged.RaiseCollectionReset );
		}

		#endregion

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		/// <summary>
		/// Отписывается от событий вложенной коллекции.
		/// </summary>
		public void Dispose()
		{
			unsubscruber.Dispose();
		}

		public CompositeDisposable unsubscriber { get; set; }
	}
}
