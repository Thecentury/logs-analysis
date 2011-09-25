using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using LogAnalyzer.Caching;
using LogAnalyzer.Kernel;

namespace LogAnalyzer
{
	[DebuggerDisplay( "Dir {Path}" )]
	internal sealed class FileSystemDirectoryInfo : IDirectoryInfo
	{
		private readonly LogDirectoryConfigurationInfo directoryConfig;
		private readonly CacheManager cacheManager;
		private readonly string path = null;
		public string Path
		{
			get { return path; }
		}

		private readonly bool useCache;

		private readonly LogNotificationsSourceBase notificationSource = null;

		public FileSystemDirectoryInfo( LogDirectoryConfigurationInfo directoryConfig )
		{
			if ( directoryConfig == null )
				throw new ArgumentNullException( "directoryConfig" );
			this.directoryConfig = directoryConfig;

			this.path = directoryConfig.Path;
			this.useCache = directoryConfig.UseCache;

			string filesFilter = directoryConfig.FileNameFilter;
			NotifyFilters notifyFilter = NotifyFilters.Size | NotifyFilters.DirectoryName | NotifyFilters.FileName;

			this.notificationSource = new DelayedLogRecordsSource( new FileSystemNotificationsSource( path, filesFilter, notifyFilter ) );

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

		public LogNotificationsSourceBase NotificationSource
		{
			get { return notificationSource; }
		}

		public IFileInfo GetFileInfo( string fullPath )
		{
			IFileInfo result = null;

			if ( directoryConfig.CustomFileCreator != null )
			{
				var context = new CreateFileInfoContext { FilePath = fullPath, UseCache = useCache };
				result = directoryConfig.CustomFileCreator( context );
				return result;
			}

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
