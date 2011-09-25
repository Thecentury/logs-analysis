﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer;

namespace ModuleLogsProvider.Logging
{
	public sealed class MostDirectoryInfo : IDirectoryInfo
	{
		private readonly LogNotificationsSourceBase notificationSource;

		public MostDirectoryInfo( LogNotificationsSourceBase notificationSource )
		{
			if ( notificationSource == null )
				throw new ArgumentNullException( "notificationSource" );

			this.notificationSource = notificationSource;
		}

		#region IDirectoryInfo Members

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

		#endregion
	}
}
