using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using LogAnalyzer.Extensions;
using System.Windows.Threading;
using System.Windows;
using System.Diagnostics;
using System.ComponentModel;

namespace LogAnalyzer.GUI.ViewModel
{
	/// <summary>
	/// Передает событие CollectionChanged в UI поток.
	/// </summary>
	[IgnoreMissingProperty( "Count" )]
	public class DispatcherObservableCollection : INotifyCollectionChanged, INotifyPropertyChanged
	{
		private readonly INotifyCollectionChanged collection = null;

		public DispatcherObservableCollection( object collection )
		{
			if ( collection == null )
				throw new ArgumentNullException( "collection" );

			INotifyCollectionChanged observableCollection = (INotifyCollectionChanged)collection;

			// todo загнать сюда IObservable как посредник
			this.collection = observableCollection;
			observableCollection.CollectionChanged += OnCollectionChanged;
		}

		private void OnCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			OnCollectionChanged( e );
		}

		// todo прореживать частоту событий
		protected virtual void OnCollectionChanged( NotifyCollectionChangedEventArgs e )
		{
			RaiseInDispatcher( e );
			PropertyChanged.Raise( this, "Count" );
		}

		private void RaiseInDispatcher( NotifyCollectionChangedEventArgs e )
		{
			var eventHandler = this.CollectionChanged;
			if ( eventHandler == null )
				return;

			Dispatcher dispatcher = Application.Current.Dispatcher;
			if ( !dispatcher.CheckAccess() )
			{
				dispatcher.BeginInvoke( eventHandler, DispatcherPriority.Background, this, e );
			}
			else
			{
				eventHandler( this, e );
			}
		}

		#region INotifyCollectionChanged Members

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public void RaiseCollectionReset()
		{
			RaiseInDispatcher( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
		}

		#endregion

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion
	}
}
