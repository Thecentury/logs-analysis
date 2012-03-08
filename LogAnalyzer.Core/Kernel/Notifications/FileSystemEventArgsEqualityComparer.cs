using System;
using System.Collections.Generic;
using System.IO;

namespace LogAnalyzer.Kernel.Notifications
{
	internal sealed class FileSystemEventArgsEqualityComparer : IEqualityComparer<EventArgs>
	{
		public bool Equals( EventArgs x, EventArgs y )
		{
			RenamedEventArgs renamedX = x as RenamedEventArgs;
			RenamedEventArgs renamedY = y as RenamedEventArgs;
			if ( renamedX != null && renamedY != null )
			{
				return true;
			}

			ErrorEventArgs errorX = x as ErrorEventArgs;
			ErrorEventArgs errorY = y as ErrorEventArgs;
			if ( errorX != null && errorY != null )
			{
				return true;
			}

			FileSystemEventArgs fsX = x as FileSystemEventArgs;
			FileSystemEventArgs fsY = y as FileSystemEventArgs;
			if ( fsX != null && fsY != null )
			{
				bool equals = fsX.ChangeType == fsY.ChangeType;
				return equals;
			}

			return false;
		}

		public int GetHashCode( EventArgs obj )
		{
			throw new NotImplementedException();
		}
	}
}