using System;
using System.IO;

namespace LogAnalyzer.Kernel.Notifications
{
	public sealed class FileSystemNotificationsSource : LogNotificationsSourceBase
	{
		private readonly FileSystemWatcher _watcher;

		public FileSystemNotificationsSource( string logsPath, string filesFilter, NotifyFilters notifyFilters, bool includeSubdirectories )
		{
			if ( !Directory.Exists( logsPath ) )
			{
				throw new InvalidOperationException( String.Format( "Directory '{0}' doesn't exist.", logsPath ) );
			}

			_watcher = new FileSystemWatcher( logsPath, filesFilter )
			{
				NotifyFilter = notifyFilters,
				IncludeSubdirectories = includeSubdirectories
			};

			_watcher.Changed += OnFileChanged;
			_watcher.Created += OnFileCreated;
			_watcher.Deleted += OnFileDeleted;
			_watcher.Error += OnError;
			_watcher.Renamed += OnRenamed;
		}

		protected override void StartCore()
		{
			_watcher.EnableRaisingEvents = true;
		}

		protected override void StopCore()
		{
			_watcher.EnableRaisingEvents = false;
		}

		private void OnFileChanged( object sender, FileSystemEventArgs e )
		{
			RaiseFileSystemEvent( ChangedHandler, e );
		}

		private void OnFileCreated( object sender, FileSystemEventArgs e )
		{
			RaiseFileSystemEvent( CreatedHandler, e );
		}

		private void OnFileDeleted( object sender, FileSystemEventArgs e )
		{
			RaiseFileSystemEvent( DeletedHandler, e );
		}

		private void OnError( object sender, ErrorEventArgs e )
		{
			RaiseError( e );
		}

		private void OnRenamed( object sender, RenamedEventArgs e )
		{
			RaiseRenamed( e );
		}
	}
}
