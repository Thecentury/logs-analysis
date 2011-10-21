using System;
using System.Collections.Specialized;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using LogAnalyzer.Extensions;
using System.Windows.Threading;
using System.Windows;
using System.ComponentModel;

namespace LogAnalyzer.GUI.ViewModels.Collections
{
	/// <summary>
	using System;
	using System.Linq;

	/// <summary>
	/// Передает событие CollectionChanged в UI поток.
	/// </summary>
	[IgnoreMissingProperty( "Count" )]
	public class DispatcherObservableCollection : INotifyCollectionChanged, INotifyPropertyChanged
	{
		private readonly IScheduler scheduler;
		private readonly object collection;

		public DispatcherObservableCollection( object collection, IScheduler scheduler )
		{
			if ( collection == null ) throw new ArgumentNullException( "collection" );
			if ( scheduler == null ) throw new ArgumentNullException( "scheduler" );

			this.collection = collection;
			this.scheduler = scheduler;

			INotifyCollectionChanged observableCollection = (INotifyCollectionChanged)collection;

			var collectionChanged = Observable.FromEventPattern<NotifyCollectionChangedEventArgs>( observableCollection, "CollectionChanged" )
				.ObserveOn( scheduler );

			collectionChanged.Where( e => e.EventArgs.Action == NotifyCollectionChangedAction.Add )
				//.Subscribe( e => OnCollectionChanged( e.EventArgs ) );
				.SelectMany( e => e.EventArgs.NewItems.Cast<object>() )
				.Subscribe( addedItem => OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, addedItem ) ) );

			collectionChanged.Where( e => e.EventArgs.Action != NotifyCollectionChangedAction.Add )
				.Subscribe( e => OnCollectionChanged( e.EventArgs ) );
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
	}
}
