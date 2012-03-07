using System;
using System.IO;
using LogAnalyzer.Kernel;
using LogAnalyzer.Kernel.Notifications;

namespace LogAnalyzer.Tests.Mocks
{
	public sealed class MockLogRecordsSource : LogNotificationsSourceBase
	{
		private readonly string directoryName;

		public MockLogRecordsSource( string directoryName )
		{
			if ( String.IsNullOrWhiteSpace( directoryName ) )
				throw new ArgumentException();

			this.directoryName = directoryName;
		}

		public void RaiseFileChanged( string fileName )
		{
			string theFileName = Path.GetFileName( fileName );
			RaiseFileSystemEvent( theFileName, directoryName, WatcherChangeTypes.Changed, ChangedHandler );
		}

		public void RaiseFileCreated( string fileName )
		{
			RaiseFileSystemEvent( fileName, directoryName, WatcherChangeTypes.Created, CreatedHandler );
		}
	}
}
