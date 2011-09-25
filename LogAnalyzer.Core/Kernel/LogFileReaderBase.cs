using System;
using System.Collections.Generic;
using LogAnalyzer.Extensions;

namespace LogAnalyzer.Kernel
{
	public abstract class LogFileReaderBase
	{
		/// <summary>
		/// Дочитать до конца, вызывается после получения уведомления о том, что файл изменился.
		/// </summary>
		/// <param name="lastAddedEntry"></param>
		/// <returns></returns>
		public abstract IList<LogEntry> ReadToEnd( LogEntry lastAddedEntry );

		/// <summary>
		/// Считать все содержимое файла.
		/// </summary>
		/// <returns></returns>
		public virtual IList<LogEntry> ReadEntireFile()
		{
			var addedEntries = ReadToEnd( lastAddedEntry: null );
			return addedEntries;
		}

		public event EventHandler<FileReadEventArgs> FileReadProgress;

		protected void RaiseFileReadProgress( int bytesReadDelta )
		{
			FileReadProgress.Raise( this, new FileReadEventArgs { BytesReadSincePreviousCall = bytesReadDelta } );
		}
	}
}