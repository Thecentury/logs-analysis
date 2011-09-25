using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Extensions;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.Tests
{
	internal sealed class MockDirectoryInfo : IDirectoryInfo
	{
		private readonly List<MockFileInfo> files = new List<MockFileInfo>();
		private readonly Encoding encoding = Encoding.Unicode;

		public List<MockFileInfo> Files
		{
			get { return files; }
		}

		private readonly MockLogRecordsSource recordsSource = null;

		private readonly string path = null;
		public string Path
		{
			get { return path; }
		}

		public MockDirectoryInfo( string path )
		{
			this.path = path;
			this.recordsSource = new MockLogRecordsSource( path );
		}

		public IEnumerable<IFileInfo> EnumerateFiles( string searchPattern )
		{
			return files;
		}

		public LogNotificationsSourceBase NotificationSource
		{
			get { return recordsSource; }
		}

		public MockFileInfo AddFile( string name )
		{
			if ( files.Any( f => f.Name == name ) )
				throw new InvalidOperationException( "File with name \"{0}\" already exists.".Format2( name ) );

			MockFileInfo fileInfo = new MockFileInfo( name, System.IO.Path.Combine( path, name ), recordsSource, encoding );
			files.Add( fileInfo );

			recordsSource.RaiseFileCreated( name );

			return fileInfo;
		}

		public IFileInfo GetFileInfo( string fullPath )
		{
			return files.Single( f => f.FullName == fullPath );
		}
	}
}
