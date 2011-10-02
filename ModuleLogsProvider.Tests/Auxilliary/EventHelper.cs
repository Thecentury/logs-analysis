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

		public abstract bool HaveNotBeenInvoked();
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

		public override bool HaveNotBeenInvoked()
		{
			return prevCalledTimes == calledTimes;
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
			return children.All( child => child.HaveBeenInvokedOneTime() );
		}

		public override bool HaveNotBeenInvoked()
		{
			return children.All( child => child.HaveNotBeenInvoked() );
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
			Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
				h => { obj.PropertyChanged += h; },
				h => { obj.PropertyChanged -= h; } )
				.Subscribe( e => countHelper.OnInvoked() );

			return countHelper;
		}
	}
}
