using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using LogAnalyzer.Caching;
using LogAnalyzer.Config;
using LogAnalyzer.Kernel.Notifications;
using LogAnalyzer.Properties;

namespace LogAnalyzer.Kernel
{
	[DebuggerDisplay( "Dir {Path}" )]
	internal class FileSystemDirectoryInfo : IDirectoryInfo
	{
		private readonly LogDirectoryConfigurationInfo _directoryConfig;
		public LogDirectoryConfigurationInfo DirectoryConfig
		{
			get { return _directoryConfig; }
		}

		private readonly CacheManager _cacheManager;
		private readonly string _path;
		public string Path
		{
			get { return _path; }
		}

		private readonly bool _useCache;

		private LogNotificationsSourceBase _notificationSource;

		public FileSystemDirectoryInfo( LogDirectoryConfigurationInfo directoryConfig )
		{
			if ( directoryConfig == null )
			{
				throw new ArgumentNullException( "directoryConfig" );
			}

			this._directoryConfig = directoryConfig;

			this._path = directoryConfig.Path;
			this._useCache = directoryConfig.UseCache;

			if ( _useCache )
			{
				_cacheManager = CacheManager.ForDirectory( directoryConfig );
			}
		}

		public virtual IEnumerable<IFileInfo> EnumerateFiles(  )
		{
			var searchOption = SearchOption;

			return Directory.EnumerateFiles( _path, "*", searchOption ).Select( GetFileInfo );
		}

		private SearchOption SearchOption
		{
			get
			{
				SearchOption searchOption = _directoryConfig.IncludeNestedDirectories
												? SearchOption.AllDirectories
												: SearchOption.TopDirectoryOnly;
				return searchOption;
			}
		}

		public IEnumerable<string> EnumerateFileNames()
		{
			var searchOption = SearchOption;

			return Directory.EnumerateFiles( _path, "*", searchOption );
		}

		protected virtual LogNotificationsSourceBase CreateNotificationSource( string filesFilter )
		{
			TimeSpan pollInterval = _directoryConfig.PollingIntervalMillisecods == 0
										? Settings.Default.FileSystemPollInterval
										: TimeSpan.FromMilliseconds( _directoryConfig.PollingIntervalMillisecods );

			return
				new PausableNotificationSource(
					new CompositeLogNotificationsSource(
						new DelayedLogRecordsSource(
							new FileSystemNotificationsSource( _path, filesFilter,
															  NotifyFilters.Size | NotifyFilters.DirectoryName | NotifyFilters.FileName,
															  _directoryConfig.IncludeNestedDirectories )
							),
						new PollingFileSystemNotificationSource( _path, filesFilter, _directoryConfig.IncludeNestedDirectories, pollInterval )
						) );
		}

		public LogNotificationsSourceBase NotificationSource
		{
			get
			{
				if ( _notificationSource == null )
				{
					_notificationSource = CreateNotificationSource( _directoryConfig.FileNameFilter );
				}
				return _notificationSource;
			}
		}

		public IFileInfo GetFileInfo( string fullPath )
		{
			IFileInfo result;

			if ( _useCache )
			{
				result = _cacheManager.CreateFile( fullPath );
			}
			else
			{
				result = new FileSystemFileInfo( fullPath );
			}

			return result;
		}
	}
}
