using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Extensions;
using LogAnalyzer.Kernel;
using LogAnalyzer.Kernel.Notifications;

namespace LogAnalyzer.Tests.Mocks
{
	public sealed class MockDirectoryInfo : IDirectoryInfo
	{
		private readonly List<MockFileInfo> _files = new List<MockFileInfo>();
		private readonly Encoding _encoding = Encoding.Unicode;

		public List<MockFileInfo> Files
		{
			get { return _files; }
		}

		private readonly MockLogRecordsSource _recordsSource;
		public MockLogRecordsSource MockNotificationSource
		{
			get { return _recordsSource; }
		}

		private readonly string _path;
		public string Path
		{
			get { return _path; }
		}

		public MockDirectoryInfo( string path )
		{
			this._path = path;
			_recordsSource = new MockLogRecordsSource( path );
		}

		public IEnumerable<IFileInfo> EnumerateFiles( string searchPattern )
		{
			return _files;
		}

		private LogNotificationsSourceBase _notificationsSource;
		public LogNotificationsSourceBase NotificationSource
		{
			get
			{
				if ( _notificationsSource == null )
				{
					_notificationsSource = _recordsSource;
					if ( CreateRecordsSourceHandler != null )
					{
						_notificationsSource = CreateRecordsSourceHandler( _recordsSource );
					}
				}

				return _notificationsSource;
			}
		}

		public Func<LogNotificationsSourceBase, LogNotificationsSourceBase> CreateRecordsSourceHandler { get; set; }

		public MockFileInfo AddFile( string name )
		{
			if ( _files.Any( f => f.Name == name ) )
				throw new InvalidOperationException( "File with name \"{0}\" already exists.".Format2( name ) );

			MockFileInfo fileInfo = new MockFileInfo( name, System.IO.Path.Combine( _path, name ), _recordsSource, _encoding );
			_files.Add( fileInfo );

			_recordsSource.RaiseFileCreated( name );

			return fileInfo;
		}

		public IFileInfo GetFileInfo( string fullPath )
		{
			return _files.Single( f => f.FullName == fullPath );
		}
	}
}
