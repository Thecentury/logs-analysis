using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ionic.Zip;
using JetBrains.Annotations;
using LogAnalyzer.Config;
using LogAnalyzer.Kernel;
using IOPath = System.IO.Path;

namespace LogAnalyzer.Zip
{
	internal sealed class ZipDirectoryInfo : IDirectoryInfo
	{
		private readonly LogNotificationsSourceBase notificationsSource = new LogNotificationsSourceBase();
		private readonly string zipFileName;
		private readonly string rootDirectory;
		private readonly DirectoriesHierarchyHelper directoriesHierarchyHelper;

		public ZipDirectoryInfo( [NotNull] LogDirectoryConfigurationInfo config, [NotNull] string zipFileName, string rootDirectory )
		{
			if ( config == null ) throw new ArgumentNullException( "config" );
			if ( zipFileName == null ) throw new ArgumentNullException( "zipFileName" );

			this.zipFileName = zipFileName;
			this.rootDirectory = rootDirectory;
			this.directoriesHierarchyHelper = new DirectoriesHierarchyHelper( rootDirectory, config.IncludeNestedDirectories );
		}

		public LogNotificationsSourceBase NotificationSource
		{
			get { return notificationsSource; }
		}

		public IEnumerable<IFileInfo> EnumerateFiles( string searchPattern )
		{
			List<ZipFileInfo> files = new List<ZipFileInfo>();

			using ( var zip = new ZipFile( zipFileName ) )
			{
				foreach ( ZipEntry zipEntry in zip )
				{
					if ( zipEntry.IsDirectory )
						continue;

					string extension = IOPath.GetExtension( zipEntry.FileName );
					if ( !String.Equals( extension, ".log", StringComparison.InvariantCultureIgnoreCase ) )
						continue;

					string dir = IOPath.GetDirectoryName( zipEntry.FileName );
					bool includeByDir = IncludeByDirectory( dir );
					if ( !includeByDir )
						continue;

					ZipFileInfo file = new ZipFileInfo( zipFileName, zipEntry.FileName );
					files.Add( file );
				}
			}

			return files;
		}

		private bool IncludeByDirectory( string directoryName )
		{
			return directoriesHierarchyHelper.IncludeDirectory( directoryName );
		}

		public IFileInfo GetFileInfo( string fullPath )
		{
			return new ZipFileInfo( zipFileName, fullPath );
		}

		public string Path
		{
			get { return zipFileName; }
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
