using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Text;
using System.Reactive.Linq;
using System.Threading;

namespace LogAnalyzer.Extensions
{
	public static class ObservableExtensions
	{
		/// <summary>
		/// Возвращает задержанную последовательность - если за указанный промежуток времени происходит несколько событий,
		/// то возвращается только последнее из них.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="delay"></param>
		/// <returns></returns>
		public static IObservable<T> Delayed<T>( this IObservable<T> source, TimeSpan delay )
		{
			return Delayed( source, delay, Scheduler.ThreadPool );
		}

		/// <summary>
		/// Возвращает задержанную последовательность - если за указанный промежуток времени происходит несколько событий,
		/// то возвращается только последнее из них.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="delay"></param>
		/// <param name="scheduler"></param>
		/// <returns></returns>
		public static IObservable<T> Delayed<T>( this IObservable<T> source, TimeSpan delay, IScheduler scheduler )
		{
			return Observable.Create<T>( observer =>
			{
				object sync = new object();
				T value;
				bool hasValue = false;

				source.Subscribe( item =>
				{
					bool hadValue;
					lock ( sync )
					{
						hadValue = hasValue;

						hasValue = true;
						value = item;
					}

					if ( !hadValue )
					{
						scheduler.Schedule( delay, () =>
						{
							lock ( sync )
							{
								observer.OnNext( value );
								hasValue = false;
							}
						} );
					}
				} );

				return () => { };
			} );
		}

		public static IObservable<EventPattern<NotifyCollectionChangedEventArgs>> ToNotifyCollectionChangedObservable(
			this INotifyCollectionChanged collection )
		{
			return Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
				h => collection.CollectionChanged += h,
				h => collection.CollectionChanged -= h );
		}

		public static IDisposable WaitForCount<T>( this IObservable<T> observable, int times )
		{
			CountdownEvent evt = new CountdownEvent( times );
			var subscription = observable.Take( times ).Subscribe( e => evt.Signal() );

			return Disposable.Create( () =>
			{
				evt.Wait();
				subscription.Dispose();
			} );
		}
	}
}
