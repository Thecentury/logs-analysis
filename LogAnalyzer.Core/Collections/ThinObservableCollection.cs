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
			CollectionChanged.RaiseCollectionChanged( this, new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, addedItem ) );
			RaiseCountChanged();
		}

		public void RaiseCollectionAdded( IList addedItems )
		{
			CollectionChanged.RaiseCollectionChanged( this, new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, addedItems ) );
			RaiseCountChanged();
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
