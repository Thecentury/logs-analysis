using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Reactive.Linq;

namespace LogAnalyzer.Extensions
{
	public static class ObservableExtensions
	{
		public static IObservable<T> Delayed<T>( this IObservable<T> source, TimeSpan delay )
		{
			return Delayed( source, delay, Scheduler.ThreadPool );
		}

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
	}
}
