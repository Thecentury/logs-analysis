using System;
using JetBrains.Annotations;

namespace LogAnalyzer
{
	public sealed class LogFileIndex
	{
		private readonly IndexRecord[] _records;
		private readonly long _lastRecordEnd;

		public LogFileIndex( [NotNull] IndexRecord[] records, long lastRecordEnd )
		{
			if ( records == null )
			{
				throw new ArgumentNullException( "records" );
			}
			_records = records;
			_lastRecordEnd = lastRecordEnd;
		}

		public IndexRecord[] Records
		{
			get { return _records; }
		}

		public long LastRecordEnd
		{
			get { return LastRecordEnd; }
		}

		public long GetEnd( long recordIndex )
		{
			if ( recordIndex < _records.Length - 1 )
			{
				return _records[recordIndex + 1].Offset;
			}
			else
			{
				return _lastRecordEnd;
			}
		}
	}
}