using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using JetBrains.Annotations;

namespace LogAnalyzer.Kernel.Notifications
{
	internal sealed class FileNameFilteringNotificationSource : LogNotificationsSourceBase
	{
		private readonly LogNotificationsSourceBase _inner;
		private readonly HashSet<string> _fileNames;

		public FileNameFilteringNotificationSource( LogNotificationsSourceBase inner, IEnumerable<string> fileNames )
		{
			if ( inner == null )
			{
				throw new ArgumentNullException( "inner" );
			}
			if ( fileNames == null )
			{
				throw new ArgumentNullException( "fileNames" );
			}

			this._inner = inner;
			this._fileNames = new HashSet<string>( fileNames );

			CreateObservable( "Changed" ).Subscribe( RaiseChanged );
			CreateObservable( "Deleted" ).Subscribe( RaiseChanged );
			CreateObservable( "Created" ).Subscribe( RaiseChanged );
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

		private IObservable<FileSystemEventArgs> CreateObservable( string eventName )
		{
			return Observable.FromEventPattern<FileSystemEventArgs>( _inner, eventName )
				.Select( e => e.EventArgs )
				.Where( e => _fileNames.Contains( e.FullPath ) );
		}
	}
}
