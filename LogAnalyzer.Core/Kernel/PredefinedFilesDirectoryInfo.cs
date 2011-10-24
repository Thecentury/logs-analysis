using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Config;

namespace LogAnalyzer.Kernel
{
	internal sealed class PredefinedFilesDirectoryInfo : FileSystemDirectoryInfo
	{
		public PredefinedFilesDirectoryInfo( LogDirectoryConfigurationInfo config ) : base( config ) { }

		protected override LogNotificationsSourceBase CreateNotificationSource( string path, string filesFilter )
		{
			//FileNameFilteringNotificationSource
			return base.CreateNotificationSource( path, filesFilter );
		}
	}
}
