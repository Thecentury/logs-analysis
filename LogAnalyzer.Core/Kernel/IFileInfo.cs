using System;

namespace LogAnalyzer.Kernel
{
	public interface IFileInfo
	{
		void Refresh();

		/// <summary>
		/// Должен вызываться только один раз для заданного LogFile, и затем кешироваться!
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		LogFileReaderBase GetReader( LogFileReaderArguments args );

		/// <summary>
		/// Длина файла, в байтах.
		/// </summary>
		int Length { get; }
		string Name { get; }
		string FullName { get; }

		/// <summary>
		/// Расширение (с начальной точкой).
		/// </summary>
		string Extension { get; }

		// todo they say this is useless - check if it is so.
		DateTime LastWriteTime { get; }

		/// <summary>
		/// Gets the date when this log file was written to.
		/// </summary>
		DateTime LoggingDate { get; }
	}
}
