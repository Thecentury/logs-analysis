using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using LogAnalyzer.Extensions;
using LogAnalyzer.Properties;

namespace LogAnalyzer.Kernel.Notifications
{
	public sealed class DelayedLogRecordsSource : LogNotificationsSourceBase
	{
		private readonly LogNotificationsSourceBase _inner;
		private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds( 500 );

		public DelayedLogRecordsSource( LogNotificationsSourceBase inner ) : this( inner, Settings.Default.FileSystemNotificationsDelayInterval ) { }

		public DelayedLogRecordsSource( LogNotificationsSourceBase inner, TimeSpan updateInterval )
		{
			if ( inner == null )
				throw new ArgumentNullException( "inner" );

			this._inner = inner;
			this._updateInterval = updateInterval;

			CreateDelayed( "Changed" ).Subscribe( RaiseChanged );
			CreateObservable( "Deleted" ).Subscribe( RaiseDeleted );
			CreateObservable( "Created" ).Subscribe( RaiseCreated );
			Observable.FromEventPattern<RenamedEventArgs>( inner, "Renamed" ).Subscribe( e => RaiseRenamed( e.EventArgs ) );
			Observable.FromEventPattern<ErrorEventArgs>( inner, "Error" ).Subscribe( e => RaiseError( e.EventArgs ) );
		}

		protected override void StartCore()
		{
			base.StartCore();
			_inner.Start();
		}

		protected override void StopCore()
		{
			_inner.Stop();
			base.StopCore();
		}

		protected override IEnumerable<LogNotificationsSourceBase> GetChildren()
		{
			yield return _inner;
		}

		private IObservable<FileSystemEventArgs> CreateDelayed( string eventName )
		{
			return Observable.FromEventPattern<FileSystemEventArgs>( _inner, eventName )
				.Select( e => e.EventArgs )
				.GroupBy( e => e.FullPath )
				.SelectMany( g => g.Delayed( _updateInterval ) );
		}

		private IObservable<FileSystemEventArgs> CreateObservable( string eventName )
		{
			return Observable.FromEventPattern<FileSystemEventArgs>( _inner, eventName )
				.Select( e => e.EventArgs );
		}
	}
}
