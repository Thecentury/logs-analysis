using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using LogAnalyzer.Kernel;

namespace LogAnalyzer
{
	public sealed class FileSystemNotificationsSource : LogNotificationsSourceBase
	{
		private readonly FileSystemWatcher watcher = null;

		public FileSystemNotificationsSource( string logsPath, string filesFilter, NotifyFilters notifyFilters )
		{
			watcher = new FileSystemWatcher( logsPath, filesFilter );
			watcher.NotifyFilter = notifyFilters;
			watcher.IncludeSubdirectories = true;

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
