using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using LogAnalyzer;
using LogAnalyzer.Kernel;
using ModuleLogsProvider.Logging.MostLogsServices;

namespace ModuleLogsProvider.Logging.Most
{
	public sealed class MostDirectoryInfo : IDirectoryInfo
	{
		private readonly MostNotificationSource notificationSource;

		public MostDirectoryInfo( MostNotificationSource notificationSource )
		{
			if ( notificationSource == null )
				throw new ArgumentNullException( "notificationSource" );

			this.notificationSource = notificationSource;
		}

		public LogNotificationsSourceBase NotificationSource
		{
			get { return notificationSource; }
		}

		public IEnumerable<IFileInfo> EnumerateFiles( string searchPattern )
		{
			List<IFileInfo> files = new List<IFileInfo>();

			var logNames = notificationSource.MessagesStorage.GetLogFileNames();
			foreach ( string logName in logNames )
			{
				var fileInfo = GetFileInfo( logName );
				files.Add( fileInfo );
			}

			return files;
		}

		public IFileInfo GetFileInfo( string fullPath )
		{
			string logName = Path.GetFileNameWithoutExtension( fullPath );

			var logEntries = notificationSource.MessagesStorage.GetEntriesByName( logName );
			MostFileInfo fileInfo = new MostFileInfo( logName, logEntries );
			return fileInfo;
		}
	}
}
