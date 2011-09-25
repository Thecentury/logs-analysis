using System;
using System.Collections.Generic;
using LogAnalyzer;
using LogAnalyzer.Kernel;

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
			throw new NotImplementedException();
		}

		public IFileInfo GetFileInfo( string fullPath )
		{
			throw new NotImplementedException();
		}
	}
}
