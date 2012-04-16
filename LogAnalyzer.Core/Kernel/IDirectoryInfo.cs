using System.Collections.Generic;
using LogAnalyzer.Kernel.Notifications;

namespace LogAnalyzer.Kernel
{
	public interface IDirectoryInfo
	{
		LogNotificationsSourceBase NotificationSource { get; }
		IEnumerable<IFileInfo> EnumerateFiles();
		IEnumerable<string> EnumerateFileNames();
		IFileInfo GetFileInfo( string fullPath );

		string Path { get; }
	}
}
