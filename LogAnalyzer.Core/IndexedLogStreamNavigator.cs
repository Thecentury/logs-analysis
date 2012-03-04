using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LogAnalyzer.Collections;
using LogAnalyzer.Kernel;

namespace LogAnalyzer
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

		public IBidirectionalEnumerator<LogEntry> GetBidirectionalEnumerator()
		{
			return GetRandomAccessEnumerator();
		}

		public IRandomAccessEnumerator<LogEntry> GetRandomAccessEnumerator()
		{
			return new IndexedLogStreamEnumerator( _stream, _encoding, _index, _parser, disposeStream: false );
		}

		public IEnumerator<LogEntry> GetEnumerator()
		{
			return GetBidirectionalEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}