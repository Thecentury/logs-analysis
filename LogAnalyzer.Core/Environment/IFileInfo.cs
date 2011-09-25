using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LogAnalyzer
{
	public interface IFileInfo
	{
		void Refresh();
		Stream OpenStream( int startPosition );

		int Length { get; }
		string Name { get; }
		string FullName { get; }
		string Extension { get; }

		// todo they say this is useless - check if it is so.
		DateTime LastWriteTime { get; }

		/// <summary>
		/// Gets the date when this log file was written to.
		/// </summary>
		DateTime LoggingDate { get; }
	}

	public interface IStreamFileInfo
	{
		Stream OpenStream( int startPosition );
	}
}
