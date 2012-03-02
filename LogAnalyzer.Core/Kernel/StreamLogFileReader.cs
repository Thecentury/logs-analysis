using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LogAnalyzer.Extensions;
using LogAnalyzer.Filters;
using LogAnalyzer.Logging;

namespace LogAnalyzer.Kernel
{
	public sealed class StreamLogFileReader : LogFileReaderBase
	{
		private readonly IFilter<LogEntry> _globalEntriesFilter;

		private long _lastLineBreakByteIndex;
		private long _prevStreamLength;
		private readonly Logger _logger;
		private readonly Encoding _encoding;
		private int _linesCount;
		private bool _lastLineWasEmpty;
		private LogEntry _lastCreatedEntry;
		private readonly LogFile _parentLogFile;
		private readonly IStreamProvider _streamProvider;
		private readonly ILogLineParser _lineParser;

		private string Name
		{
			get { return _parentLogFile.Name; }
		}

		public StreamLogFileReader( LogFileReaderArguments args, IStreamProvider streamProvider )
		{
			if ( args == null ) throw new ArgumentNullException( "args" );
			if ( streamProvider == null ) throw new ArgumentNullException( "streamProvider" );

			args.Validate();

			_logger = args.Logger;
			_parentLogFile = args.ParentLogFile;
			_encoding = args.Encoding;
			_globalEntriesFilter = args.GlobalEntriesFilter;
			_lineParser = args.LineParser;
			_streamProvider = streamProvider;
		}

		private Stream OpenStreamAtPosition( int startingPosition )
		{
			return _streamProvider.OpenStream( startingPosition );
		}

		private StreamReader OpenReader( Stream stream )
		{
			return new StreamReader( stream, _encoding );
		}

		private List<LogEntry> NoLogEntries()
		{
			return new List<LogEntry>();
		}

		private static bool WasLineBreakAtTheEnd( string str )
		{
			if ( String.IsNullOrEmpty( str ) )
				return false;

			char lastChar = str[str.Length - 1];
			return lastChar == '\r' || lastChar == '\n';
		}

		#region ILogFileReader Members

		private string ReadAddedText()
		{
			string addedText;
			using ( Stream fs = OpenStreamAtPosition( (int)_lastLineBreakByteIndex ) )
			{
				// todo use ReadTimeout

				// длина файла уменьшилась - пока рассматриваем только append
				if ( fs.Length < _prevStreamLength )
				{
					_logger.WriteError( "LogFile.OnFileChanged: Длина файла \"{0}\" уменьшилась с {1} до {2}", Name, _prevStreamLength, fs.Length );
					return null;
				}

				if ( fs.Length == _prevStreamLength )
				{
					// длина файла не изменилась
					_logger.DebugWriteInfo( "LogFile.OnFileChanged: Длина файла \"{0}\" не изменилась ({1} байт)", Name, _prevStreamLength );
					return null;
				}

				long streamLength = fs.Length;

				// todo асинхронное чтение тут и в ReadFile

				// начинаем читать с начала последней строки
				fs.Position = this._lastLineBreakByteIndex;
				using ( StreamReader reader = OpenReader( fs ) )
				{
					addedText = reader.ReadToEnd();
					bool wasLineBreakAtTheEnd = WasLineBreakAtTheEnd( addedText );

					if ( wasLineBreakAtTheEnd )
					{
						this._lastLineBreakByteIndex = streamLength;
					}
					else
					{
						int breakLineIndexInAddedText = addedText.LastIndexOf( '\n' );
						if ( breakLineIndexInAddedText > -1 )
						{
							// e.g.:
							// |--n-| Length = 4, LineBreakIndex = 2, charsCountToTheEnd = 1 == 4 - (2 + 1).
							int charsFromLineBreakToTheEndOfString = addedText.Length - (breakLineIndexInAddedText + 1);
							char[] chars = addedText.ToCharArray( breakLineIndexInAddedText + 1, charsFromLineBreakToTheEndOfString );

							int bytesCountFromLastLineBreakToTheEndOfAddedText = _encoding.GetByteCount( chars );

							this._lastLineBreakByteIndex += (addedText.Length - bytesCountFromLastLineBreakToTheEndOfAddedText);
						}
					}
				}

				this._prevStreamLength = streamLength;
			}

			return addedText;
		}

		public override IList<LogEntry> ReadToEnd( LogEntry lastAddedEntry )
		{
			string addedText = ReadAddedText();

			if ( String.IsNullOrEmpty( addedText ) )
			{
				_logger.DebugWriteInfo( "LogFile.OnFileChanged: Файл \"{0}\": добавленный текст пуст.", this.Name );
				return NoLogEntries();
			}

			long lineIndex = Math.Max( this._linesCount - 1, 0 );

			string[] addedLines = addedText.Split( new[] { Environment.NewLine }, StringSplitOptions.None );

			var addedEntries = new List<LogEntry>();

			foreach ( var line in addedLines )
			{
				Condition.DebugAssert( !(line.Contains( '\r' ) || line.Contains( '\n' )), "Added text contains new line characters." );

				bool emptyLine = String.IsNullOrEmpty( line );
				if ( !emptyLine )
				{
					LogEntryAppendResult _ = AppendLine( line, lineIndex, lastAddedEntry, addedEntries );
				}

				_linesCount = (int)(lineIndex + 1);
				lineIndex++;
				_lastLineWasEmpty = emptyLine;
			}

			return addedEntries;
		}

		/// <summary>
		/// Максимальное число не распарсенных записей, после прочтения которых будет брошено исключение InvalidEncodingException.
		/// </summary>
		private const int MaxLinesReadWhenThrowInvalidEncodingException = 250;
		private const int FileReadNotificationBytesStep = 512000;

		/// <summary>
		/// Максимальная длина одной считанной из файла строки.
		/// <para/>Если длина строки больше, то считается, что выбрана неправильная кодировка, и бросаетcя исключение InvalidEncodingException.
		/// </summary>
		private const int MaxLineLength = 100000;

		public override IList<LogEntry> ReadEntireFile()
		{
			try
			{
				return ReadEntireFileUnsafe();
			}
			catch ( Exception exc )
			{
				_logger.WriteError( "Exception while reading file '{0}' {1}", this.Name, exc );
				throw;
			}
		}

		private sealed class ReadResult
		{
			public List<LogEntry> Entries { get; set; }
			public long StartOffset { get; set; }
		}

		private ReadResult ReadChunk( int startStreamPosition, int endStreamPosition, bool isFirst )
		{
			List<LogEntry> logEntries = new List<LogEntry>();
			ReadResult result = new ReadResult { Entries = logEntries };

			using ( Stream stream = OpenStreamAtPosition( startStreamPosition ) )
			{
				using ( StreamReader reader = OpenReader( stream ) )
				{
					string line;

					if ( !isFirst )
					{
						line = reader.ReadLine();
						int byteCount = _encoding.GetByteCount( line );
						result.StartOffset = startStreamPosition + byteCount;
					}
					else
					{
						result.StartOffset = startStreamPosition;
					}

					while ( (line = reader.ReadLine()) != null && stream.Position < endStreamPosition )
					{
						// todo brinchuk разобраться с нулем
						LogEntryAppendResult lineAppendResult = AppendLine( line, 0, logEntries.LastOrDefault(), logEntries );
					}
				}
			}

			return result;
		}

		private IList<LogEntry> ReadChunked()
		{
			List<LogEntry> logEntries = new List<LogEntry>();

			int chunksNum = Environment.ProcessorCount;
			int chunkLength;
			using ( var stream = OpenStreamAtPosition( 0 ) )
			{
				chunkLength = (int)(stream.Length / (double)chunksNum);
			}

			ReadResult[] results = new ReadResult[chunksNum];
			Action[] actions = new Action[chunksNum];
			for ( int i = 0; i < chunksNum; i++ )
			{
				int start = chunkLength * i;
				int end = start + chunkLength;
				int iLocal = i;
				Action action = () => results[iLocal] = ReadChunk( start, end, iLocal == 0 );
				actions[i] = action;
			}

			Parallel.Invoke( actions );

			foreach ( var result in results )
			{
				logEntries.AddRange( result.Entries );
			}

			return logEntries;
		}

		private IList<LogEntry> ReadEntireFileUnsafe()
		{
			// todo brinchuk comment me
			//var result = ReadChunked();
			//return result;

			List<LogEntry> logEntries = new List<LogEntry>();

			using ( Stream stream = OpenStreamAtPosition( 0 ) )
			{
				int notificationsCount = 0;
				long lineIndex = 0;

				int length = (int)stream.Length;
				if ( length == 0 )
				{
					return logEntries;
				}

				using ( StreamReader reader = OpenReader( stream ) )
				{
					string line;
					string prevLine = String.Empty;
					long prevLineBreakIndex;
					int notParsedLinesCount = 0;
					int bytesReadDelta;
					while ( (line = reader.ReadLine()) != null )
					{
						LogEntryAppendResult lineAppendResult = AppendLine( line, lineIndex, logEntries.LastOrDefault(), logEntries );

						if ( (notificationsCount + 1) * FileReadNotificationBytesStep < stream.Position )
						{
							bytesReadDelta = (int)(stream.Position - notificationsCount * FileReadNotificationBytesStep);

							RaiseFileReadProgress( bytesReadDelta );
							notificationsCount++;
						}

						if ( lineAppendResult == LogEntryAppendResult.NotParsed )
						{
							notParsedLinesCount++;
						}
						else
						{
							notParsedLinesCount = 0;
						}

						lineIndex++;
						prevLine = line;

						bool invalidEncoding = notParsedLinesCount > MaxLinesReadWhenThrowInvalidEncodingException && logEntries.Count == 0
											   || line.Length > MaxLineLength;

						if ( invalidEncoding )
						{
							throw new InvalidEncodingException( _encoding, Name );
						}
					}

					bytesReadDelta = (int)(stream.Position - notificationsCount * FileReadNotificationBytesStep);
					RaiseFileReadProgress( bytesReadDelta );

					bool wasLineBreak = WasLineBreakAtTheEnd( prevLine );
					if ( wasLineBreak )
					{
						prevLineBreakIndex = length;
					}
					else
					{
						// если последняя считанная строка не заканчивается переносом строки,
						// то считаем, что перенос строки был перед началом последней строки.
						int byteCount = _encoding.GetByteCount( prevLine );
						prevLineBreakIndex = length - byteCount;
					}

					this._lastLineBreakByteIndex = prevLineBreakIndex;
				}

				_linesCount = (int)lineIndex;

				if ( _lastLineBreakByteIndex == length )
				{
					_linesCount++;
					_lastLineWasEmpty = true;
				}
			}

			return logEntries;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="line"></param>
		/// <param name="lineIndex"></param>
		/// <param name="latestAddedEntry"></param>
		/// <param name="addedEntries"></param>
		/// <returns>Была ли на самом деле добавлена новая строка?</returns>
		private LogEntryAppendResult AppendLine( string line, long lineIndex, LogEntry latestAddedEntry, IList<LogEntry> addedEntries )
		{
			if ( line == null )
				throw new ArgumentNullException( "line" );

			// некорректные значения
			string type = null;
			int tid = 0;
			DateTime time = default( DateTime );
			string lineText = null;

			bool isLastLineChanged = lineIndex == (_linesCount - 1) && _linesCount > 0 && !_lastLineWasEmpty;

			// обновилась последняя строка файла?
			if ( isLastLineChanged )
			{
				// в последней строке было начало LogEntry?
				if ( _lineParser.TryExtractLogEntryData( line, ref type, ref tid, ref time, ref lineText ) )
				{
					LogEntry lastEntry = _lastCreatedEntry;
					// последняя запись состоит только из заголовка?
					if ( lastEntry.LinesCount == 1 )
					{
						lastEntry.UpdateHeader( type, tid, time, lineText );
						_logger.DebugWriteVerbose( "LogFile.AppendLine: File = \"{0}\": updated header in line #{1}", Name, lineIndex );
					}
					else // неполный заголовок новой записи был ошибочно дописан в текст предыдущей записи -> исправляем
					{
						lastEntry.TextLines.RemoveLastItem();
						lastEntry.Freeze();

						// после удаления последней строки самая последняя запись перестала пропускаться фильтром?
						if ( !_globalEntriesFilter.Include( lastEntry ) )
						{
							RemoveLastEntry( addedEntries );
						}

						_lastCreatedEntry = new LogEntry( type, tid, time, lineText, (int)lineIndex, _parentLogFile );
						TryAddEntry( _lastCreatedEntry, addedEntries );
					}
				}
				else
				{
					if ( latestAddedEntry != null )
					{
						latestAddedEntry.ReplaceLastLine( line, lineIndex );
						_logger.DebugWriteVerbose( "LogFile.AppendLine: File = \"{0}\": replaced line #{1}", Name, lineIndex );
					}
				}
			}
			else // добавилась новая строка
			{
				// в добавленной строке было начало нового LogEntry?
				if ( _lineParser.TryExtractLogEntryData( line, ref type, ref tid, ref time, ref lineText ) )
				{
					_lastCreatedEntry = new LogEntry( type, tid, time, lineText, (int)lineIndex, _parentLogFile );

					LogEntryAppendResult result = TryAddEntry( _lastCreatedEntry, addedEntries );
					if ( result == LogEntryAppendResult.Added )
					{
						// морозим предыдущий, если он был
						if ( latestAddedEntry != null )
						{
							latestAddedEntry.Freeze();
						}
					}
				}
				else
				{
					// добавляем к предыдущей записи лога
					if ( _lastCreatedEntry != null )
					{
						_lastCreatedEntry.AppendLine( line );

						bool wasNotAddedToList = _lastCreatedEntry != latestAddedEntry;
						if ( wasNotAddedToList )
						{
							LogEntryAppendResult res = TryAddEntry( _lastCreatedEntry, addedEntries );
							if ( res == LogEntryAppendResult.Added )
							{
								_logger.DebugWriteVerbose( "LogFile.AppendLine: File = \"{0}\": Appended line #{1} with text \"{2}\"", Name, lineIndex, line );
							}
							return res;
						}
					}

					// нет последней линии - например, мы читаем файл и все линии до этого не проходили по дате
					// или неправильная кодировка - тогда в файл не будет добавлена ни одна запись
					if ( _lastCreatedEntry == null )
						return LogEntryAppendResult.NotParsed;
				}
			}

			return LogEntryAppendResult.Added;
		}

		private LogEntryAppendResult TryAddEntry( LogEntry newEntry, IList<LogEntry> addedEntries )
		{
			if ( _globalEntriesFilter.Include( newEntry ) )
			{
				addedEntries.Add( newEntry );

				return LogEntryAppendResult.Added;
			}
			else
			{
				return LogEntryAppendResult.ExcludedByFilter;
			}
		}


		private void RemoveLastEntry( IList<LogEntry> addedEntries )
		{
			addedEntries.RemoveLastItem();
		}

		#endregion
	}
}
