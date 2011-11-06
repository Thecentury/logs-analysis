using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using LogAnalyzer.Properties;

namespace LogAnalyzer.Kernel
{
	/// <summary>
	/// Реализация класса, уведомляющего об изменениях в файловой системе, которая периодически опрашивает файловую систему на наличие изменений.
	/// </summary>
	public sealed class PollingFileSystemNotificationSource : LogNotificationsSourceBase
	{
		private readonly string logsPath;
		private readonly string filesFilter;
		private readonly bool includeSubdirectories;
		private readonly Timer timer;
		private readonly object sync = new object();
		private HashSet<FileInfo> files = new HashSet<FileInfo>();

		public PollingFileSystemNotificationSource( string logsPath, string filesFilter, bool includeSubdirectories )
			: this( logsPath, filesFilter, includeSubdirectories, Settings.Default.FileSystemPollInterval ) { }

		public PollingFileSystemNotificationSource( string logsPath, string filesFilter, bool includeSubdirectories, TimeSpan updateInterval )
		{
			if ( !Directory.Exists( logsPath ) )
				throw new InvalidOperationException( string.Format( "Directory '{0}' doesn't exist.", logsPath ) );

			this.logsPath = logsPath;
			this.filesFilter = filesFilter;
			this.includeSubdirectories = includeSubdirectories;

			timer = new Timer { Interval = updateInterval.TotalMilliseconds, Enabled = false };
			timer.Elapsed += OnTimerElapsed;
		}

		private HashSet<FileInfo> GetFilesSnapshot()
		{
			var currentFiles = Directory.GetFiles( logsPath, filesFilter,
				includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly );
			var snapshot = new HashSet<FileInfo>( currentFiles.Select( f => new FileInfo( f ) ) );

			return snapshot;
		}

		private void OnTimerElapsed( object sender, ElapsedEventArgs e )
		{
			lock ( sync )
			{
				var current = GetFilesSnapshot();

				var added = GetAdded( current, files );
				foreach ( var fileInfo in added )
				{
					RaiseCreated( new FileSystemEventArgs( WatcherChangeTypes.Created, logsPath, fileInfo.Name ) );
				}

				var deleted = GetDeleted( current, files );
				foreach ( var fileInfo in deleted )
				{
					RaiseDeleted( new FileSystemEventArgs( WatcherChangeTypes.Deleted, logsPath, fileInfo.Name ) );
				}

				files = current;

				foreach ( var fileInfo in files )
				{
					var length = fileInfo.Length;
					fileInfo.Refresh();
					var actualLength = fileInfo.Length;

					if ( actualLength != length )
					{
						RaiseChanged( new FileSystemEventArgs( WatcherChangeTypes.Changed, logsPath, fileInfo.Name ) );
					}
				}
			}
		}

		private IEnumerable<FileInfo> GetAdded( IEnumerable<FileInfo> current, HashSet<FileInfo> prev )
		{
			return current.Where( f => !prev.Contains( f ) );
		}

		private IEnumerable<FileInfo> GetDeleted( HashSet<FileInfo> current, IEnumerable<FileInfo> prev )
		{
			return prev.Where( f => !current.Contains( f ) );
		}

		protected override void StartCore()
		{
			base.StartCore();
			timer.Enabled = true;

			lock ( sync )
			{
				files = GetFilesSnapshot();
			}
		}

		protected override void StopCore()
		{
			base.StopCore();
			timer.Enabled = false;
		}
	}
}
