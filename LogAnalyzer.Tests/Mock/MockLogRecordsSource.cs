using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.Tests
{
	public sealed class MockLogRecordsSource : LogNotificationsSourceBase
	{
		private readonly string directoryName = null;

		public MockLogRecordsSource( string directoryName )
		{
			if ( String.IsNullOrWhiteSpace( directoryName ) )
				throw new ArgumentException();

			this.directoryName = directoryName;
		}

		public void RaiseFileChanged( string fileName )
		{
			RaiseFileSystemEvent( fileName, directoryName, WatcherChangeTypes.Changed, ChangedHandler );
		}

		public void RaiseFileCreated( string fileName )
		{
			RaiseFileSystemEvent( fileName, directoryName, WatcherChangeTypes.Created, CreatedHandler );
		}
	}
}
