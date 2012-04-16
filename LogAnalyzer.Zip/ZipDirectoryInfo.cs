using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ionic.Zip;
using JetBrains.Annotations;
using LogAnalyzer.Config;
using LogAnalyzer.Kernel;
using LogAnalyzer.Kernel.Notifications;
using IOPath = System.IO.Path;

namespace LogAnalyzer.Zip
{
	internal sealed class ZipDirectoryInfo : IDirectoryInfo
	{
		private readonly LogNotificationsSourceBase _notificationsSource = new LogNotificationsSourceBase();
		private readonly string _zipFileName;
		[UsedImplicitly]
		private readonly string _rootDirectory;
		private readonly DirectoriesHierarchyHelper _directoriesHierarchyHelper;

		public ZipDirectoryInfo( [NotNull] LogDirectoryConfigurationInfo config, [NotNull] string zipFileName, string rootDirectory )
		{
			if ( config == null )
			{
				throw new ArgumentNullException( "config" );
			}
			if ( zipFileName == null )
			{
				throw new ArgumentNullException( "zipFileName" );
			}

			this._zipFileName = zipFileName;
			this._rootDirectory = rootDirectory;
			this._directoriesHierarchyHelper = new DirectoriesHierarchyHelper( rootDirectory, config.IncludeNestedDirectories );
		}

		public LogNotificationsSourceBase NotificationSource
		{
			get { return _notificationsSource; }
		}

		public IEnumerable<IFileInfo> EnumerateFiles()
		{
			List<ZipFileInfo> files = new List<ZipFileInfo>();

			foreach ( ZipEntry zipEntry in EnumerateZipEntries() )
			{
				ZipFileInfo file = new ZipFileInfo( _zipFileName, zipEntry.FileName );
				files.Add( file );
			}

			return files;
		}

		private IEnumerable<ZipEntry> EnumerateZipEntries()
		{
			using ( var zip = new ZipFile( _zipFileName ) )
			{
				foreach ( ZipEntry zipEntry in zip )
				{
					if ( zipEntry.IsDirectory )
					{
						continue;
					}

					string extension = IOPath.GetExtension( zipEntry.FileName );
					if ( !String.Equals( extension, ".log", StringComparison.InvariantCultureIgnoreCase ) )
					{
						continue;
					}

					string dir = IOPath.GetDirectoryName( zipEntry.FileName );
					if ( dir == null )
					{
						continue;
					}

					dir = dir.Replace( '\\', '/' );
					bool includeByDir = IncludeByDirectory( dir );
					if ( !includeByDir )
					{
						continue;
					}

					yield return zipEntry;
				}
			}
		}

		public IEnumerable<string> EnumerateFileNames()
		{
			foreach ( ZipEntry zipEntry in EnumerateZipEntries() )
			{
				yield return zipEntry.FileName;
			}
		}

		private bool IncludeByDirectory( string directoryName )
		{
			return _directoriesHierarchyHelper.IncludeDirectory( directoryName );
		}

		public IFileInfo GetFileInfo( string fullPath )
		{
			return new ZipFileInfo( _zipFileName, fullPath );
		}

		public string Path
		{
			get { return _zipFileName; }
		}

		internal sealed class DirectoriesHierarchyHelper
		{
			private readonly string rootDirectory;
			private readonly bool includeNestedDirectories;

			public DirectoriesHierarchyHelper( string rootDirectory, bool includeNestedDirectories )
			{
				this.rootDirectory = rootDirectory;
				this.includeNestedDirectories = includeNestedDirectories;
			}

			public bool IncludeDirectory( string directory )
			{
				if ( rootDirectory == null )
					return true;

				if ( includeNestedDirectories )
					return directory.StartsWith( rootDirectory, StringComparison.InvariantCultureIgnoreCase );
				else
					return String.Equals( directory, rootDirectory, StringComparison.InvariantCultureIgnoreCase );
			}
		}
	}
}
