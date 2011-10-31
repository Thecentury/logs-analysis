using System.Collections.Generic;

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
