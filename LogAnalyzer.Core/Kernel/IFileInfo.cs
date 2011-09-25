using System;

namespace LogAnalyzer.Kernel
{
	public interface IFileInfo
	{
		void Refresh();

		ILogFileReader GetReader( LogFileReaderArguments args );

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
}
