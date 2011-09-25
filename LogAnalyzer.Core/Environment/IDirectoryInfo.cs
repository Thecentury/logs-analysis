using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer
{
	// todo probably add a UseCache property
	public interface IDirectoryInfo
	{
		LogNotificationsSourceBase NotificationSource { get; }
		IEnumerable<IFileInfo> EnumerateFiles( string searchPattern );
		IFileInfo GetFileInfo( string fullPath );
	}
}
