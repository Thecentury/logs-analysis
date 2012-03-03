using System;
using System.Collections;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using LogAnalyzer.Collections;
using LogAnalyzer.Kernel;
using LogAnalyzer.Misc;

namespace LogAnalyzer
{
	public interface IStreamReaderFactory
	{
		TextReader CreateReader( Stream stream, Encoding encoding );
	}

	public sealed class FuncStreamReaderFactory : IStreamReaderFactory
	{
		private readonly Func<Stream, Encoding, TextReader> _creater;

		public FuncStreamReaderFactory( Func<Stream, Encoding, TextReader> creater )
		{
			_creater = creater;
		}

		public TextReader CreateReader( Stream stream, Encoding encoding )
		{
			TextReader reader = _creater( stream, encoding );
			return reader;
		}
	}

	public sealed class LogFileNavigator : IBidirectionalEnumerable<LogEntry>
	{
		private readonly IFileInfo _fileInfo;
		private readonly LogFileReaderArguments _parameters;
		private readonly IStreamReaderFactory _streamReaderFactory;
		private readonly Encoding _encoding;
		private readonly ILogLineParser _lineParser;

		public LogFileNavigator( [NotNull] IFileInfo fileInfo, [NotNull] LogFileReaderArguments parameters )
			: this( fileInfo, parameters, new FuncStreamReaderFactory( ( stream, encoding ) => new StreamReader( stream, encoding ) ) )
		{
		}

		public LogFileNavigator( [NotNull] IFileInfo fileInfo, [NotNull] LogFileReaderArguments parameters, [NotNull] IStreamReaderFactory streamReaderFactory )
		{
			if ( fileInfo == null )
			{
				throw new ArgumentNullException( "fileInfo" );
			}
			if ( parameters == null )
			{
				throw new ArgumentNullException( "parameters" );
			}
			if ( streamReaderFactory == null )
			{
				throw new ArgumentNullException( "streamReaderFactory" );
			}
			if ( parameters.Encoding == null )
			{
				throw new ArgumentException( "parameters.Encoding" );
			}
			if ( parameters.LineParser == null )
			{
				throw new ArgumentException( "parameters.LineParser" );
			}

			_fileInfo = fileInfo;
			_parameters = parameters;
			_streamReaderFactory = streamReaderFactory;
			_encoding = parameters.Encoding;
			_lineParser = parameters.LineParser;
		}

		public IBidirectionalEnumerator<LogEntry> GetEnumerator()
		{
			return new Enumerator( this );
		}

		private sealed class Enumerator : IBidirectionalEnumerator<LogEntry>
		{
			private readonly LogFileNavigator _parent;
			private readonly Stream _stream;
			private readonly TextReader _reader;
			private readonly ILogLineParser _parser;

			private string _type;
			private int _threadId;
			private DateTime _time;
			private string _text;
			private LogEntry _logEntry;
			private int _lineIndex = -1;
			private bool _hasReadHeader;

			public Enumerator( LogFileNavigator parent )
			{
				_parent = parent;
				_parser = parent._lineParser;

				_stream = _parent._fileInfo.OpenStream();
				_reader = _parent._streamReaderFactory.CreateReader( _stream, _parent._encoding );
			}

			public bool MoveBack()
			{
				throw new NotImplementedException();
			}

			public void Dispose()
			{
				_reader.Dispose();
				_stream.Dispose();
			}

			public bool MoveNext()
			{
				bool logEntryHeaderRead;

				if ( !_hasReadHeader )
				{
					do
					{
						string line = _reader.ReadLine();
						_lineIndex++;
						if ( line == null )
						{
							return false;
						}

						logEntryHeaderRead = _parser.TryExtractLogEntryData( line, ref _type, ref _threadId, ref _time, ref _text );
					} while ( !logEntryHeaderRead );

					_hasReadHeader = true;
				}

				_logEntry = new LogEntry( _type, _threadId, _time, _text, _lineIndex, _parent._parameters.ParentLogFile );

				do
				{
					string line = _reader.ReadLine();
					_lineIndex++;
					if ( line == null )
					{
						_hasReadHeader = false;
						break;
					}

					logEntryHeaderRead = _parser.TryExtractLogEntryData( line, ref _type, ref _threadId, ref _time, ref _text );
					if ( !logEntryHeaderRead )
					{
						_hasReadHeader = false;
						_logEntry.AppendLine( line );
					}
					else
					{
						_hasReadHeader = true;
					}

				} while ( !logEntryHeaderRead );

				return true;
			}

			public void Reset()
			{
				throw new NotSupportedException();
			}

			public LogEntry Current
			{
				get { return _logEntry; }
			}

			object IEnumerator.Current
			{
				get { return Current; }
			}
		}
	}

	public sealed class LogFileIndexer
	{
		private readonly LogFileReaderArguments _arguments;

		public LogFileIndex BuildIndex( IFileInfo file )
		{
			throw new NotImplementedException();
		}
	}

	public sealed class LogFileIndex
	{
		private readonly IndexRecord[] _records;

		public LogFileIndex( [NotNull] IndexRecord[] records )
		{
			if ( records == null )
			{
				throw new ArgumentNullException( "records" );
			}
			_records = records;
		}

		public IndexRecord[] Records
		{
			get { return _records; }
		}
	}

	public struct IndexRecord
	{
		public long Offset { get; set; }
	}
}