using System;
using System.Collections;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using LogAnalyzer.Collections;
using LogAnalyzer.Kernel;

namespace LogAnalyzer
{
	public sealed class IndexedLogStreamEnumerator : IRandomAccessEnumerator<LogEntry>
	{
		private readonly Stream _stream;
		private readonly LogFileIndex _logFileIndex;
		private readonly LogStreamReader _logStreamReader;
		private readonly bool _disposeStream;

		private long _index = -1;
		private LogEntry _currentLogEntry;

		public IndexedLogStreamEnumerator( [NotNull] Stream stream, [NotNull] Encoding encoding, [NotNull] LogFileIndex logFileIndex,
		                                   [NotNull] ILogLineParser parser, bool disposeStream )
		{
			if ( stream == null )
			{
				throw new ArgumentNullException( "stream" );
			}
			if ( encoding == null )
			{
				throw new ArgumentNullException( "encoding" );
			}
			if ( logFileIndex == null )
			{
				throw new ArgumentNullException( "logFileIndex" );
			}
			if ( parser == null )
			{
				throw new ArgumentNullException( "parser" );
			}

			_stream = stream;
			_logFileIndex = logFileIndex;
			_disposeStream = disposeStream;
			_logStreamReader = new LogStreamReader( stream, encoding, parser );
		}

		public bool MoveBack()
		{
			if ( _index == 0 )
			{
				return false;
			}

			_index--;

			ReadLogEntry();

			return true;
		}

		private void ReadLogEntry()
		{
			_stream.Position = _logFileIndex.Records[_index].Offset;
			_currentLogEntry = _logStreamReader.ReadTo( _logFileIndex.GetEnd( _index ) );
		}

		public void Dispose()
		{
			if ( _disposeStream )
			{
				_stream.Dispose();
			}
		}

		public bool MoveNext()
		{
			if ( _index == _logFileIndex.Records.LongLength - 1 )
			{
				return false;
			}

			_index++;

			ReadLogEntry();

			return true;
		}

		public void Reset()
		{
			throw new NotSupportedException();
		}

		public LogEntry Current
		{
			get { return _currentLogEntry; }
		}

		object IEnumerator.Current
		{
			get { return Current; }
		}

		public long Position
		{
			get { return _index; }
			set
			{
				if ( value < 0 || value >= _logFileIndex.Records.LongLength )
				{
					throw new ArgumentOutOfRangeException();
				}

				_index = value;
				ReadLogEntry();
			}
		}
	}
}