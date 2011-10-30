using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using LogAnalyzer.Config;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.Zip
{
	public sealed class ZipDirectoryInfo : IDirectoryInfo
	{
		private readonly LogNotificationsSourceBase notificationsSource = new LogNotificationsSourceBase();

		public ZipDirectoryInfo( LogDirectoryConfigurationInfo config )
		{
			ZipInputStream stream = new ZipInputStream( null );
			ZipEntry entry = stream.GetNextEntry();
		}

		public LogNotificationsSourceBase NotificationSource
		{
			get { return notificationsSource; }
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
