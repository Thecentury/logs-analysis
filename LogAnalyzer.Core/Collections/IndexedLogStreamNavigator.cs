using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.Collections
{
	public sealed class IndexedLogStreamNavigator : IRandomAccessEnumerable<LogEntry>
	{
		private readonly LogFileIndex _index;
		private readonly Stream _stream;
		private readonly Encoding _encoding;
		private readonly ILogLineParser _parser;

		public IndexedLogStreamNavigator( Stream stream, Encoding encoding, ILogLineParser parser, LogFileIndex index )
		{
			_stream = stream;
			_encoding = encoding;
			_parser = parser;
			_index = index;
		}

		IBidirectionalEnumerator<LogEntry> IBidirectionalEnumerable<LogEntry>.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IRandomAccessEnumerator<LogEntry> GetEnumerator()
		{
			return new IndexedLogStreamEnumerator( _stream, _encoding, _index, _parser, disposeStream: false );
		}

		IEnumerator<LogEntry> IEnumerable<LogEntry>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}