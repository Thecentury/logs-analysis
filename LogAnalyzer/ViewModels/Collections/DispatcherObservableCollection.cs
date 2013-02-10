using System;
using System.Collections;
using System.Collections.Specialized;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using LogAnalyzer.Extensions;
using System.ComponentModel;
using LogAnalyzer.Logging;

namespace LogAnalyzer.GUI.ViewModels.Collections
{
	/// <summary>
	/// Передает событие CollectionChanged в UI поток.
	/// </summary>
	[IgnoreMissingProperty( "Count" )]
	public class DispatcherObservableCollection : INotifyCollectionChanged, INotifyPropertyChanged, IDisposable, IEnumerable
	{
		private readonly IScheduler _scheduler;
		private readonly IEnumerable _collection;
		private readonly CompositeDisposable _unsubscriber;

		public string Name { get; set; }

		public DispatcherObservableCollection( IEnumerable collection, IScheduler scheduler )
		{
			if ( collection == null )
			{
				throw new ArgumentNullException( "collection" );
			}
			if ( scheduler == null )
			{
				throw new ArgumentNullException( "scheduler" );
			}

			this._collection = collection;
			this._scheduler = scheduler;

			INotifyCollectionChanged observableCollection = (INotifyCollectionChanged)collection;

			var observable = Observable.FromEventPattern<NotifyCollectionChangedEventArgs>( observableCollection,
																						   "CollectionChanged" );

			var unsubscribeAdd = observable.Where( e => e.EventArgs.Action == NotifyCollectionChangedAction.Add )
				.Buffer( TimeSpan.FromSeconds( 1 ) )
				.Where( e => e.Count > 0 )
				.ObserveOn( scheduler )
				.SelectMany( e => e )
				.Subscribe( e => OnAdded( e.EventArgs ) );

			var collectionChanged = observable
				.ObserveOn( scheduler );
			var unsubscribeOthers = collectionChanged.Where( e => e.EventArgs.Action != NotifyCollectionChangedAction.Add )
				.Subscribe( e => OnCollectionChanged( e.EventArgs ) );

			_unsubscriber = new CompositeDisposable( unsubscribeAdd, unsubscribeOthers );
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
			_scheduler.Schedule( CollectionChanged.RaiseCollectionReset );
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
			_unsubscriber.Dispose();
		}

		public IEnumerator GetEnumerator()
		{
			return _collection.GetEnumerator();
		}
	}
}
