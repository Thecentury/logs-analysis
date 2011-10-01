using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using LogAnalyzer.Extensions;
using System.ComponentModel;
using System.Collections;

namespace LogAnalyzer
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

		public void RaiseCollectionItemsAdded( IList addedItems )
		{
			CollectionChanged.RaiseCollectionChanged( this, new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, addedItems ) );
			RaiseCountChanged();
		}

		public void RaiseCollectionItemsAdded<T>( IList<T> addedItems )
		{
			RaiseCollectionItemsAdded( (IList)addedItems );
		}

		#endregion

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		protected void RaiseCountChanged()
		{
			PropertyChanged.Raise( this, "Count" );
		}

		#endregion
	}
}
