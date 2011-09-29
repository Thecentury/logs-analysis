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
		private readonly MostLogNotificationSource notificationSource;
		private readonly Dictionary<string, MostFileInfo> namesToFilesMap = new Dictionary<string, MostFileInfo>();

		public MostDirectoryInfo( MostLogNotificationSource notificationSource )
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
			// todo brinchuk this is only for test purposes
			// GetOrCreateFile( "L1" );

			List<IFileInfo> files = new List<IFileInfo>();

			var logNames = notificationSource.MessagesStorage.GetLogFileNames();
			foreach ( string logName in logNames )
			{
				MostFileInfo fileInfo = GetOrCreateFile( logName );
				files.Add( fileInfo );
			}

			return files;
		}

		private MostFileInfo GetOrCreateFile( string logName )
		{
			MostFileInfo fileInfo;
			if ( !namesToFilesMap.TryGetValue( logName, out fileInfo ) )
			{
				fileInfo = CreateFile( logName );
				namesToFilesMap.Add( logName, fileInfo );
			}

			return fileInfo;
		}

		public IFileInfo GetFileInfo( string fullPath )
		{
			string logName = Path.GetFileNameWithoutExtension( fullPath );

			return GetOrCreateFile( logName );
		}

		private MostFileInfo CreateFile( string logName )
		{
			var logEntries = notificationSource.MessagesStorage.GetEntriesByName( logName );
			MostFileInfo fileInfo = new MostFileInfo( logName, logEntries );
			return fileInfo;
		}
	}
}
