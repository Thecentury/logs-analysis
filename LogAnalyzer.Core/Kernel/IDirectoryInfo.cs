using System.Collections.Generic;
using LogAnalyzer.Kernel.Notifications;

namespace LogAnalyzer.Kernel
{
	public interface IDirectoryInfo
	{
		LogNotificationsSourceBase NotificationSource { get; }
		IEnumerable<IFileInfo> EnumerateFiles( string searchPattern );
		IFileInfo GetFileInfo( string fullPath );

		string Path { get; }
	}
}
