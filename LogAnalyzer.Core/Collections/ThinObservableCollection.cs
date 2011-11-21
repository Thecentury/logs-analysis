using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using LogAnalyzer.Extensions;
using System.ComponentModel;
using System.Collections;

namespace LogAnalyzer.Collections
{
	public abstract class ThinObservableCollection : INotifyCollectionChanged, INotifyPropertyChanged
	{
		#region INotifyCollectionChanged Members

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public void RaiseCollectionReset()
		{
			CollectionChanged.RaiseCollectionReset( this );
			RaiseCountChanged();
		}

		public void RaiseCollectionAdded( object addedItem )
		{
			if ( addedItem is IList )
			{
				throw new InvalidOperationException( "Use RaiseCollectionItemsAdded instead." );
			}

			CollectionChanged.RaiseCollectionChanged( this, new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, addedItem ) );
			RaiseCountChanged();
		}

		public void RaiseCollectionItemsAdded( IList addedItems, int startingIndex )
		{
			var args = new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, addedItems, startingIndex );
			CollectionChanged.RaiseCollectionChanged( this, args );
			RaiseCountChanged();
		}

		public void RaiseGenericCollectionItemsAdded<T>( IList<T> addedItems, int startingIndex )
		{
			RaiseCollectionItemsAdded( (IList)addedItems, startingIndex );
		}

		#endregion

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		public void RaiseCountChanged()
		{
			PropertyChanged.Raise( this, "Count" );
		}

		#endregion
	}
}
