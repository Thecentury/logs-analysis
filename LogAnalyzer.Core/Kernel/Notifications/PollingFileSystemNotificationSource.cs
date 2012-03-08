using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using LogAnalyzer.Properties;

namespace LogAnalyzer.Kernel.Notifications
{
	/// <summary>
	/// Реализация класса, уведомляющего об изменениях в файловой системе, которая периодически опрашивает файловую систему на наличие изменений.
	/// </summary>
	public sealed class PollingFileSystemNotificationSource : LogNotificationsSourceBase
	{
		private readonly string _logsPath;
		private readonly string _filesFilter;
		private readonly bool _includeSubdirectories;
		private readonly Timer _timer;
		private readonly object _sync = new object();
		private HashSet<FileInfo> _files = new HashSet<FileInfo>();
		private HashSet<string> _fileNames = new HashSet<string>();

		public PollingFileSystemNotificationSource( string logsPath, string filesFilter, bool includeSubdirectories )
			: this( logsPath, filesFilter, includeSubdirectories, Settings.Default.FileSystemPollInterval ) { }

		public PollingFileSystemNotificationSource( string logsPath, string filesFilter, bool includeSubdirectories, TimeSpan updateInterval )
		{
			if ( !Directory.Exists( logsPath ) )
				throw new InvalidOperationException( string.Format( "Directory '{0}' doesn't exist.", logsPath ) );

			this._logsPath = logsPath;
			this._filesFilter = filesFilter;
			this._includeSubdirectories = includeSubdirectories;

			_timer = new Timer { Interval = updateInterval.TotalMilliseconds };
			_timer.Elapsed += OnTimerElapsed;
		}

		private HashSet<FileInfo> GetFilesSnapshot()
		{
			var currentFiles = Directory.GetFiles( _logsPath, _filesFilter,
				_includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly );

			var snapshot = new HashSet<FileInfo>( currentFiles.Select( f => new FileInfo( f ) ) );

			return snapshot;
		}

		private HashSet<string> GetFileNamesSnapshot()
		{
			return
				new HashSet<string>(
					Directory.EnumerateFiles( _logsPath, _filesFilter,
						_includeSubdirectories
						? SearchOption.AllDirectories
						: SearchOption.TopDirectoryOnly ) );
		}

		private void OnTimerElapsed( object sender, ElapsedEventArgs e )
		{
			lock ( _sync )
			{
				var currentFileNames = GetFileNamesSnapshot();

				var added = GetAdded( currentFileNames, _fileNames );
				foreach ( var fullPath in added )
				{
					RaiseCreated( new FileSystemEventArgs( WatcherChangeTypes.Created,
						Path.GetDirectoryName( fullPath ), Path.GetFileName( fullPath ) ) );

					_files.Add( new FileInfo( fullPath ) );
				}

				var deleted = GetDeleted( currentFileNames, _fileNames );
				foreach ( var fullPath in deleted )
				{
					RaiseDeleted( new FileSystemEventArgs( WatcherChangeTypes.Deleted,
						Path.GetDirectoryName( fullPath ), Path.GetFileName( fullPath ) ) );

					_files.RemoveWhere( f => f.FullName == fullPath );
				}

				_fileNames = currentFileNames;

				foreach ( var fileInfo in _files )
				{
					var prevLength = fileInfo.Length;
					fileInfo.Refresh();
					var currentLength = fileInfo.Length;

					if ( currentLength != prevLength )
					{
						RaiseChanged( new FileSystemEventArgs( WatcherChangeTypes.Changed, _logsPath, fileInfo.Name ) );
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
			lock ( _sync )
			{
				_files = GetFilesSnapshot();
			}

			base.StartCore();
			_timer.Start();
		}

		protected override void StopCore()
		{
			base.StopCore();
			_timer.Stop();
		}
	}
}
