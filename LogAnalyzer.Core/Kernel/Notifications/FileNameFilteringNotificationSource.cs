using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using LogAnalyzer.Kernel.Notifications;

namespace LogAnalyzer.Kernel
{
	internal sealed class FileNameFilteringNotificationSource : LogNotificationsSourceBase
	{
		private readonly LogNotificationsSourceBase inner;
		private readonly HashSet<string> fileNames = new HashSet<string>();

		public FileNameFilteringNotificationSource( LogNotificationsSourceBase inner, IEnumerable<string> fileNames )
		{
			if ( inner == null ) throw new ArgumentNullException( "inner" );
			if ( fileNames == null ) throw new ArgumentNullException( "fileNames" );

			this.inner = inner;
			this.fileNames = new HashSet<string>( fileNames );

			CreateObservable( "Changed" ).Subscribe( RaiseChanged );
			CreateObservable( "Deleted" ).Subscribe( RaiseChanged );
			CreateObservable( "Created" ).Subscribe( RaiseChanged );
			Observable.FromEventPattern<ErrorEventArgs>( inner, "Error" ).Subscribe( e => RaiseError( e.EventArgs ) );
		}

		protected override void StartCore()
		{
			base.StartCore();
			inner.Start();
		}

		protected override void StopCore()
		{
			inner.Stop();
			base.StopCore();
		}

		private IObservable<FileSystemEventArgs> CreateObservable( string eventName )
		{
			return Observable.FromEventPattern<FileSystemEventArgs>( inner, eventName )
				.Select( e => e.EventArgs )
				.Where( e => fileNames.Contains( e.FullPath ) );
		}
	}
}
