using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using LogAnalyzer.Caching;
using LogAnalyzer.Config;
using LogAnalyzer.Kernel.Notifications;

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
				throw new ArgumentNullException( "directoryConfig" );

			this._directoryConfig = directoryConfig;

			this._path = directoryConfig.Path;
			this._useCache = directoryConfig.UseCache;

			if ( _useCache )
			{
				_cacheManager = CacheManager.ForDirectory( directoryConfig );
			}
		}

		public virtual IEnumerable<IFileInfo> EnumerateFiles( string searchPattern )
		{
			SearchOption searchOption = _directoryConfig.IncludeNestedDirectories
											? SearchOption.AllDirectories
											: SearchOption.TopDirectoryOnly;

			return Directory.EnumerateFiles( _path, searchPattern, searchOption ).Select( GetFileInfo );
		}

		protected virtual LogNotificationsSourceBase CreateNotificationSource( string filesFilter )
		{
			return
				new PausableNotificationSource(
					new CompositeLogNotificationsSource(
						new DelayedLogRecordsSource(
							new FileSystemNotificationsSource( _path, filesFilter,
															  NotifyFilters.Size | NotifyFilters.DirectoryName | NotifyFilters.FileName,
															  _directoryConfig.IncludeNestedDirectories )
							),
							new PollingFileSystemNotificationSource( _path, filesFilter, _directoryConfig.IncludeNestedDirectories )
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
