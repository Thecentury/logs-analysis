using System;
using System.IO;
using System.Reactive.Linq;
using JetBrains.Annotations;

namespace LogAnalyzer.Kernel.Notifications
{
	public abstract class SubscribableLogNotificationSourceBase : LogNotificationsSourceBase
	{
		protected void SubscribeToEvents( [NotNull] LogNotificationsSourceBase child )
		{
			if ( child == null )
			{
				throw new ArgumentNullException( "child" );
			}

			CreateObservable( child, "Changed" ).Subscribe( RaiseChanged );
			CreateObservable( child, "Created" ).Subscribe( RaiseCreated );
			CreateObservable( child, "Deleted" ).Subscribe( RaiseDeleted );
			CreateObservable<ErrorEventArgs>( child, "Error" ).Subscribe( RaiseError );
		}

		private IObservable<T> CreateObservable<T>( object child, string eventName ) where T : EventArgs
		{
			return Observable.FromEventPattern<T>( child, eventName )
				.Select( e => e.EventArgs );
		}

		private IObservable<FileSystemEventArgs> CreateObservable( object child, string eventName )
		{
			return Observable.FromEventPattern<FileSystemEventArgs>( child, eventName )
				.Select( e => e.EventArgs );
		}
	}
}