using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace ModuleLogsProvider.Tests.Auxilliary
{
	public abstract class EventCounterBase
	{
		internal abstract void OnInvoked();

		public abstract bool HaveBeenInvokedOneTime();
	}

	public sealed class EventCountHelper : EventCounterBase
	{
		private int calledTimes;
		private int prevCalledTimes;

		public int CalledTimes
		{
			get { return calledTimes; }
		}

		internal override void OnInvoked()
		{
			calledTimes++;
		}

		public override bool HaveBeenInvokedOneTime()
		{
			bool wasInvokedOneTime = prevCalledTimes == calledTimes - 1;
			prevCalledTimes = calledTimes;

			return wasInvokedOneTime;
		}
	}

	public sealed class CompositeEventCounter : EventCounterBase
	{
		private readonly List<EventCounterBase> children = new List<EventCounterBase>();

		public CompositeEventCounter( params EventCounterBase[] children )
		{
			this.children.AddRange( children );
		}

		internal override void OnInvoked()
		{
			foreach ( EventCounterBase child in children )
			{
				child.OnInvoked();
			}
		}

		public override bool HaveBeenInvokedOneTime()
		{
			foreach ( EventCounterBase child in children )
			{
				bool hasBeenInvokedOneTime = child.HaveBeenInvokedOneTime();
				if ( !hasBeenInvokedOneTime )
					return false;
			}

			return true;
		}
	}

	public static class EventCountExtensions
	{
		public static EventCountHelper CreateEventCounterFromCollectionChanged( this INotifyCollectionChanged obj )
		{
			EventCountHelper countHelper = new EventCountHelper();
			Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
				h => { obj.CollectionChanged += h; },
				h => { obj.CollectionChanged -= h; } )
				.Subscribe( e => countHelper.OnInvoked() );
			return countHelper;
		}

		public static EventCountHelper CreateEventCounterFromPropertyChanged( this INotifyPropertyChanged obj )
		{
			EventCountHelper countHelper = new EventCountHelper();
			Observable.FromEventPattern<PropertyChangedEventArgs>( obj, "PropertyChanged" )
				.Subscribe( e => countHelper.OnInvoked() );
			return countHelper;
		}
	}
}
