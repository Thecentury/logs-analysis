using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reactive.Linq;
using LogAnalyzer.Extensions;
using System.Reactive.Concurrency;
using System.Reactive;

namespace LogAnalyzer
{
	public sealed class DelayedLogRecordsSource : LogNotificationsSourceBase
	{
		private readonly LogNotificationsSourceBase inner = null;
		private readonly TimeSpan updateInterval = TimeSpan.FromMilliseconds( 500 );

		public DelayedLogRecordsSource( LogNotificationsSourceBase inner ) : this( inner, TimeSpan.FromMilliseconds( 500 ) ) { }

		public DelayedLogRecordsSource( LogNotificationsSourceBase inner, TimeSpan updateInterval )
		{
			if ( inner == null )
				throw new ArgumentNullException( "inner" );

			this.inner = inner;
			this.updateInterval = updateInterval;

			CreateDelayed( "Changed" ).Subscribe( e => RaiseChanged( e ) );
			CreateObservable( "Deleted" ).Subscribe( e => RaiseChanged( e ) );
			CreateObservable( "Created" ).Subscribe( e => RaiseChanged( e ) );
			Observable.FromEventPattern<RenamedEventArgs>( inner, "Renamed" ).Subscribe( e => RaiseRenamed( e.EventArgs ) );
			Observable.FromEventPattern<ErrorEventArgs>( inner, "Error" ).Subscribe( e => RaiseError( e.EventArgs ) );
		}

		protected override void StartCore()
		{
			inner.Start();
		}

		private IObservable<FileSystemEventArgs> CreateDelayed( string eventName )
		{
			return Observable.FromEventPattern<FileSystemEventArgs>( inner, eventName )
				.Select( e => e.EventArgs )
				.GroupBy( e => e.FullPath )
				.SelectMany( g => g.Throttle( updateInterval ) );
		}

		private IObservable<FileSystemEventArgs> CreateObservable( string eventName )
		{
			return Observable.FromEventPattern<FileSystemEventArgs>( inner, eventName )
				.Select( e => e.EventArgs );
		}
	}
}
