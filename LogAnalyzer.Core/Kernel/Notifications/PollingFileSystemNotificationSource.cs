using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using LogAnalyzer.Kernel.Notifications;
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
		private HashSet<string> fileNames = new HashSet<string>();

		public PollingFileSystemNotificationSource( string logsPath, string filesFilter, bool includeSubdirectories )
			: this( logsPath, filesFilter, includeSubdirectories, Settings.Default.FileSystemPollInterval ) { }

		public PollingFileSystemNotificationSource( string logsPath, string filesFilter, bool includeSubdirectories, TimeSpan updateInterval )
		{
			if ( !Directory.Exists( logsPath ) )
				throw new InvalidOperationException( string.Format( "Directory '{0}' doesn't exist.", logsPath ) );

			this.logsPath = logsPath;
			this.filesFilter = filesFilter;
			this.includeSubdirectories = includeSubdirectories;

			timer = new Timer { Interval = updateInterval.TotalMilliseconds };
			timer.Elapsed += OnTimerElapsed;
		}

		private HashSet<FileInfo> GetFilesSnapshot()
		{
			var currentFiles = Directory.GetFiles( logsPath, filesFilter,
				includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly );

			var snapshot = new HashSet<FileInfo>( currentFiles.Select( f => new FileInfo( f ) ) );

			return snapshot;
		}

		private HashSet<string> GetFileNamesSnapshot()
		{
			return
				new HashSet<string>(
					Directory.EnumerateFiles( logsPath, filesFilter,
						includeSubdirectories
						? SearchOption.AllDirectories
						: SearchOption.TopDirectoryOnly ) );
		}

		private void OnTimerElapsed( object sender, ElapsedEventArgs e )
		{
			lock ( sync )
			{
				var currentFileNames = GetFileNamesSnapshot();

				var added = GetAdded( currentFileNames, fileNames );
				foreach ( var fullPath in added )
				{
					RaiseCreated( new FileSystemEventArgs( WatcherChangeTypes.Created,
						Path.GetDirectoryName( fullPath ), Path.GetFileName( fullPath ) ) );

					files.Add( new FileInfo( fullPath ) );
				}

				var deleted = GetDeleted( currentFileNames, fileNames );
				foreach ( var fullPath in deleted )
				{
					RaiseDeleted( new FileSystemEventArgs( WatcherChangeTypes.Deleted,
						Path.GetDirectoryName( fullPath ), Path.GetFileName( fullPath ) ) );

					files.RemoveWhere( f => f.FullName == fullPath );
				}

				fileNames = currentFileNames;

				foreach ( var fileInfo in files )
				{
					var prevLength = fileInfo.Length;
					fileInfo.Refresh();
					var currentLength = fileInfo.Length;

					if ( currentLength != prevLength )
					{
						RaiseChanged( new FileSystemEventArgs( WatcherChangeTypes.Changed, logsPath, fileInfo.Name ) );
					}
				}
			}
		}

		private IEnumerable<string> GetAdded( IEnumerable<string> current, HashSet<string> prev )
		{
			return current.Where( f => !prev.Contains( f ) );
		}

		private IEnumerable<string> GetDeleted( HashSet<string> current, IEnumerable<string> prev )
		{
			return prev.Where( f => !current.Contains( f ) );
		}

		protected override void StartCore()
		{
			lock ( sync )
			{
				files = GetFilesSnapshot();
			}

			base.StartCore();
			timer.Start();
		}

		protected override void StopCore()
		{
			base.StopCore();
			timer.Stop();
		}
	}
}
