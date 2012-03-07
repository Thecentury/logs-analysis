using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;

namespace LogAnalyzer.Kernel.Notifications
{
	public sealed class CompositeLogNotificationsSource : LogNotificationsSourceBase
	{
		private readonly List<LogNotificationsSourceBase> _children = new List<LogNotificationsSourceBase>();

		public CompositeLogNotificationsSource( params LogNotificationsSourceBase[] children )
			: this( (IEnumerable<LogNotificationsSourceBase>)children ) { }

		public CompositeLogNotificationsSource( IEnumerable<LogNotificationsSourceBase> children )
		{
			if ( children == null ) throw new ArgumentNullException( "children" );

			this._children.AddRange( children );

			foreach ( var child in this._children )
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

		protected override void StartCore()
		{
			base.StartCore();

			foreach (var child in _children)
			{
				child.Start();
			}
		}

		protected override void StopCore()
		{
			foreach (var child in _children)
			{
				child.Stop();
			}

			base.StopCore();
		}
	}
}
