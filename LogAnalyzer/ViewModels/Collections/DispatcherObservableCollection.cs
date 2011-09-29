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
	/// Передает событие CollectionChanged в UI поток.
	/// </summary>
	[IgnoreMissingProperty( "Count" )]
	public class DispatcherObservableCollection : INotifyCollectionChanged, INotifyPropertyChanged
	{
		private readonly IScheduler scheduler;

		public DispatcherObservableCollection( object collection, IScheduler scheduler )
		{
			if ( collection == null ) throw new ArgumentNullException( "collection" );
			if ( scheduler == null ) throw new ArgumentNullException( "scheduler" );

			this.scheduler = scheduler;

			INotifyCollectionChanged observableCollection = (INotifyCollectionChanged)collection;

			Observable.FromEvent
				<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
					h => observableCollection.CollectionChanged += h,
					h => observableCollection.CollectionChanged -= h )
				.ObserveOn( scheduler )
				.Subscribe( OnCollectionChanged );
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
