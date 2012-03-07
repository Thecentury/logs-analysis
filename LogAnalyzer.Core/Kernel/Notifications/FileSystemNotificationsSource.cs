using System;
using System.IO;
using LogAnalyzer.Kernel.Notifications;

namespace LogAnalyzer.Kernel
{
	public sealed class FileSystemNotificationsSource : LogNotificationsSourceBase
	{
		private readonly FileSystemWatcher watcher;

		public FileSystemNotificationsSource( string logsPath, string filesFilter, NotifyFilters notifyFilters, bool includeSubdirectories )
		{
			if ( !Directory.Exists( logsPath ) )
				throw new InvalidOperationException( string.Format( "Directory '{0}' doesn't exist.", logsPath ) );

			watcher = new FileSystemWatcher( logsPath, filesFilter )
			{
				NotifyFilter = notifyFilters,
				IncludeSubdirectories = includeSubdirectories
			};

			watcher.Changed += OnFileChanged;
			watcher.Created += OnFileCreated;
			watcher.Deleted += OnFileDeleted;
			watcher.Error += OnError;
			watcher.Renamed += OnRenamed;
		}

		protected override void StartCore()
		{
			watcher.EnableRaisingEvents = true;
		}

		protected override void StopCore()
		{
			watcher.EnableRaisingEvents = false;
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
