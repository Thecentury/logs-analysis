using System;
using System.IO;
using System.Reactive.Linq;
using LogAnalyzer.Extensions;
using LogAnalyzer.Properties;

namespace LogAnalyzer.Kernel
{
	public sealed class DelayedLogRecordsSource : LogNotificationsSourceBase
	{
		private readonly LogNotificationsSourceBase inner;
		private readonly TimeSpan updateInterval = TimeSpan.FromMilliseconds( 500 );

		public DelayedLogRecordsSource( LogNotificationsSourceBase inner ) : this( inner, Settings.Default.FileSystemNotificationsDelayInterval ) { }

		public DelayedLogRecordsSource( LogNotificationsSourceBase inner, TimeSpan updateInterval )
		{
			if ( inner == null )
				throw new ArgumentNullException( "inner" );

			this.inner = inner;
			this.updateInterval = updateInterval;

			CreateDelayed( "Changed" ).Subscribe( RaiseChanged );
			CreateObservable( "Deleted" ).Subscribe( RaiseDeleted );
			CreateObservable( "Created" ).Subscribe( RaiseCreated );
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
				.SelectMany( g => g.Delayed( updateInterval ) );
		}

		private IObservable<FileSystemEventArgs> CreateObservable( string eventName )
		{
			return Observable.FromEventPattern<FileSystemEventArgs>( inner, eventName )
				.Select( e => e.EventArgs );
		}
	}
}
