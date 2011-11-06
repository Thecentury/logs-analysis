using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Extensions;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.Tests
{
	public sealed class MockDirectoryInfo : IDirectoryInfo
	{
		private readonly List<MockFileInfo> files = new List<MockFileInfo>();
		private readonly Encoding encoding = Encoding.Unicode;

		public List<MockFileInfo> Files
		{
			get { return files; }
		}

		private readonly MockLogRecordsSource recordsSource;
		public MockLogRecordsSource MockNotificationSource
		{
			get { return recordsSource; }
		}

		private readonly string path;
		public string Path
		{
			get { return path; }
		}

		public MockDirectoryInfo( string path )
		{
			this.path = path;
			recordsSource = new MockLogRecordsSource( path );
		}

		public IEnumerable<IFileInfo> EnumerateFiles( string searchPattern )
		{
			return files;
		}

		private LogNotificationsSourceBase notificationsSource;
		public LogNotificationsSourceBase NotificationSource
		{
			get
			{
				if ( notificationsSource == null )
				{
					notificationsSource = recordsSource;
					if ( CreateRecordsSourceHandler != null )
					{
						notificationsSource = CreateRecordsSourceHandler( recordsSource );
					}
				}

				return notificationsSource;
			}
		}

		public Func<LogNotificationsSourceBase, LogNotificationsSourceBase> CreateRecordsSourceHandler { get; set; }

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
