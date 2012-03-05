using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using LogAnalyzer.Common;
using LogAnalyzer.Kernel;
using LogAnalyzer.Misc;

namespace LogAnalyzer.Collections
{
	public sealed class LogFileNavigator : IBidirectionalEnumerable<LogEntry>
	{
		private readonly IFileInfo _fileInfo;
		private readonly LogFileReaderArguments _parameters;
		private readonly IStreamReaderFactory _streamReaderFactory;
		private readonly Encoding _encoding;
		private readonly ILogLineParser _lineParser;

		public LogFileNavigator( [NotNull] IFileInfo fileInfo, [NotNull] LogFileReaderArguments parameters )
			: this( fileInfo, parameters, new StreamReaderFactory() )
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
			return new LogStreamEnumerator( _streamReaderFactory.CreateReader( _fileInfo.OpenStream(), _encoding ), _lineParser,
				_parameters.ParentLogFile, disposeReader: false );
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

	public sealed class LogStreamEnumerator : IBidirectionalEnumerator<LogEntry>
	{
		private readonly PositionAwareStreamReader _positionAwareStreamReader;
		private readonly TextReader _reader;
		private readonly ILogLineParser _parser;
		private readonly LogFile _parentFile;
		private readonly bool _disposeReader;

		private string _type;
		private int _threadId;
		private DateTime _time;
		private string _text;
		private LogEntry _logEntry;
		private int _lineIndex = -1;
		private bool _hasReadHeader;

		public LogStreamEnumerator( [NotNull] TextReader reader, [NotNull] ILogLineParser parser, [CanBeNull] LogFile parentFile, bool disposeReader )
		{
			if ( reader == null )
			{
				throw new ArgumentNullException( "reader" );
			}
			if ( parser == null )
			{
				throw new ArgumentNullException( "parser" );
			}

			_reader = reader;
			_positionAwareStreamReader = reader as PositionAwareStreamReader;
			_parser = parser;
			_parentFile = parentFile;
			_disposeReader = disposeReader;
		}

		public bool MoveBack()
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			if ( _disposeReader )
			{
				_reader.Dispose();
			}
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

			_logEntry = new LogEntry( _type, _threadId, _time, _text, _lineIndex, _parentFile );

			do
			{
				if ( _positionAwareStreamReader != null )
				{
					_positionAwareStreamReader.SavePosition();
				}

				string line = _reader.ReadLine();
				_lineIndex++;
				if ( line == null )
				{
					_hasReadHeader = false;
					if ( _positionAwareStreamReader != null )
					{
						_positionAwareStreamReader.SavePosition();
					}
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