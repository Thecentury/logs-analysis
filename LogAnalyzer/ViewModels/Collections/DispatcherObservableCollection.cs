using System;
using System;
using System.Linq;
using System.Collections.Specialized;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using LogAnalyzer.Extensions;
using System.Windows.Threading;
using System.Windows;
using System.ComponentModel;

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

			var collectionChanged = Observable.FromEventPattern<NotifyCollectionChangedEventArgs>( observableCollection, "CollectionChanged" )
				.ObserveOn( scheduler );

			var unsubscribeAdd = collectionChanged.Where( e => e.EventArgs.Action == NotifyCollectionChangedAction.Add )
				.Subscribe( e => OnAdded( e.EventArgs ) );

			var unsubscriveOthers = collectionChanged.Where( e => e.EventArgs.Action != NotifyCollectionChangedAction.Add )
				.Subscribe( e => OnCollectionChanged( e.EventArgs ) );

			unsubscruber = new CompositeDisposable( unsubscribeAdd, unsubscriveOthers );
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

		// todo прореживать частоту событий
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
	}
}
