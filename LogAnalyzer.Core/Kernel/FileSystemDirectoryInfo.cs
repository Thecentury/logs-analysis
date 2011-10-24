﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using LogAnalyzer.Caching;
using LogAnalyzer.Config;

namespace LogAnalyzer.Kernel
{
	[DebuggerDisplay( "Dir {Path}" )]
	internal class FileSystemDirectoryInfo : IDirectoryInfo
	{
		private readonly LogDirectoryConfigurationInfo directoryConfig;
		public LogDirectoryConfigurationInfo DirectoryConfig
		{
			get { return directoryConfig; }
		}

		private readonly CacheManager cacheManager;
		private readonly string path;
		public string Path
		{
			get { return path; }
		}

		private readonly bool useCache;

		private readonly LogNotificationsSourceBase notificationSource;

		public static FileSystemDirectoryInfo Create( LogDirectoryConfigurationInfo config )
		{
			if ( config.PredefinedFiles.Count == 0 )
				return new FileSystemDirectoryInfo( config );
			else
				return new PredefinedFilesDirectoryInfo( config );
		}

		protected FileSystemDirectoryInfo( LogDirectoryConfigurationInfo directoryConfig )
		{
			if ( directoryConfig == null )
				throw new ArgumentNullException( "directoryConfig" );

			this.directoryConfig = directoryConfig;

			this.path = directoryConfig.Path;
			this.useCache = directoryConfig.UseCache;

			string filesFilter = directoryConfig.FileNameFilter;

			this.notificationSource = CreateNotificationSource( path, filesFilter );

			if ( useCache )
			{
				cacheManager = CacheManager.ForDirectory( directoryConfig );
			}
		}

		public IEnumerable<IFileInfo> EnumerateFiles( string searchPattern )
		{
			// todo дать возможность настраивать вложенные папки?
			return Directory.EnumerateFiles( path, searchPattern, SearchOption.TopDirectoryOnly ).Select( GetFileInfo );
		}

		protected virtual LogNotificationsSourceBase CreateNotificationSource( string path, string filesFilter )
		{
			return new DelayedLogRecordsSource( new FileSystemNotificationsSource( path, filesFilter,
				NotifyFilters.Size | NotifyFilters.DirectoryName | NotifyFilters.FileName ) );
		}

		public LogNotificationsSourceBase NotificationSource
		{
			get { return notificationSource; }
		}

		public IFileInfo GetFileInfo( string fullPath )
		{
			IFileInfo result;

			if ( useCache )
			{
				result = cacheManager.CreateFile( fullPath );
			}
			else
			{
				result = new FileSystemFileInfo( fullPath );
			}

			return result;
		}
	}
}
