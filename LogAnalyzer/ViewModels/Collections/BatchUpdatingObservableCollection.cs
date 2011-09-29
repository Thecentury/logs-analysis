using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace LogAnalyzer.GUI.ViewModels.Collections
{
	public sealed class BatchUpdatingObservableCollection<T> : ObservableCollection<T>
	{
		public BatchUpdatingObservableCollection() { }

		public BatchUpdatingObservableCollection( IEnumerable<T> collection )
		{
			if ( collection == null )
				throw new ArgumentNullException( "collection" );

			foreach ( var item in collection )
			{
				Add( item );
			}
		}

		private bool inBatchUpdate = false;

		public void StartBatchUpdate()
		{
			inBatchUpdate = true;
		}

		public void EndBatchUpdate( bool raiseCollectionReset )
		{
			inBatchUpdate = false;

			if ( raiseCollectionReset )
			{
				OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
			}
		}

		public void RaiseCollectionReset()
		{
			OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
		}

		protected override void OnCollectionChanged( NotifyCollectionChangedEventArgs e )
		{
			if ( !inBatchUpdate )
			{
				base.OnCollectionChanged( e );
			}
		}

		protected override void ClearItems()
		{
			foreach ( IDisposable item in this.OfType<IDisposable>() )
			{
				item.Dispose();
			}

			base.ClearItems();
		}
	}
}
