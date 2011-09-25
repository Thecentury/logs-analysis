using System;
using System.Collections.Generic;
using LogAnalyzer.Extensions;

namespace LogAnalyzer.Kernel
{
	public abstract class LogFileReaderBase
	{
		/// <summary>
		/// �������� �� �����, ���������� ����� ��������� ����������� � ���, ��� ���� ���������.
		/// </summary>
		/// <param name="lastAddedEntry"></param>
		/// <returns></returns>
		public abstract IList<LogEntry> ReadToEnd( LogEntry lastAddedEntry );

		/// <summary>
		/// ������� ��� ���������� �����.
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