using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace ModuleLogsProvider.Tests.Auxilliary
{
	public sealed class EventCountHelper
	{
		private int calledTimes = 0;
		private int prevCalledTimes = 0;

		public EventCountHelper()
		{
		}

		public int CalledTimes
		{
			get { return calledTimes; }
		}

		public void OnCalled()
		{
			calledTimes++;
		}

		public bool HaveBeenInvokedOneTime()
		{
			bool wasInvokedOneTime = prevCalledTimes == calledTimes - 1;
			prevCalledTimes = calledTimes;

			return wasInvokedOneTime;
		}
	}

	public static class EventCountExtensions
	{
		public static EventCountHelper CreateEventCounterFromCollectionChanged( this INotifyCollectionChanged obj )
		{
			EventCountHelper countHelper = new EventCountHelper();
			Observable.FromEventPattern<NotifyCollectionChangedEventArgs>( obj, "CollectionChanged" )
				.Subscribe( e => countHelper.OnCalled() );
			return countHelper;
		}

		public static EventCountHelper CreateEventCounterFromPropertyChanged( this INotifyPropertyChanged obj )
		{
			EventCountHelper countHelper = new EventCountHelper();
			Observable.FromEventPattern<PropertyChangedEventArgs>( obj, "PropertyChanged" )
				.Subscribe( e => countHelper.OnCalled() );
			return countHelper;
		}
	}
}
