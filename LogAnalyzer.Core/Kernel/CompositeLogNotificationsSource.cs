using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace LogAnalyzer.Kernel
{
	public sealed class CompositeLogNotificationsSource : LogNotificationsSourceBase
	{
		private readonly List<LogNotificationsSourceBase> children = new List<LogNotificationsSourceBase>();

		public CompositeLogNotificationsSource( params LogNotificationsSourceBase[] children )
			: this( (IEnumerable<LogNotificationsSourceBase>)children ) { }

		public CompositeLogNotificationsSource( IEnumerable<LogNotificationsSourceBase> children )
		{
			if ( children == null ) throw new ArgumentNullException( "children" );

			this.children.AddRange( children );

			foreach ( var child in this.children )
			{
				CreateObservable( child, "Changed" ).Subscribe( RaiseChanged );
				CreateObservable( child, "Created" ).Subscribe( RaiseCreated );
				CreateObservable( child, "Deleted" ).Subscribe( RaiseDeleted );
				CreateObservable<ErrorEventArgs>( child, "Error" ).Subscribe( RaiseError );
			}
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
