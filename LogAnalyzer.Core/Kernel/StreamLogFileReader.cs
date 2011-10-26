using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LogAnalyzer.Extensions;
using LogAnalyzer.Filters;
using LogAnalyzer.Logging;

namespace LogAnalyzer.Kernel
{
	public sealed class StreamLogFileReader : LogFileReaderBase
	{
		private readonly IFilter<LogEntry> globalEntriesFilter;

		private long lastLineBreakByteIndex;
		private long prevStreamLength;
		private readonly Logger logger;
		private readonly Encoding encoding;
		private int linesCount;
		private bool lastLineWasEmpty;
		private LogEntry lastCreatedEntry;
		private readonly LogFile parentLogFile;
		private readonly IStreamProvider streamFileInfo;

		private string Name
		{
			get { return parentLogFile.Name; }
		}

		public StreamLogFileReader( LogFileReaderArguments args, IStreamProvider streamFileInfo )
		{
			if ( args == null ) throw new ArgumentNullException( "args" );
			if ( streamFileInfo == null ) throw new ArgumentNullException( "streamFileInfo" );

			logger = args.Logger;
			parentLogFile = args.ParentLogFile;
			encoding = args.Encoding;
			globalEntriesFilter = args.GlobalEntriesFilter;

			this.streamFileInfo = streamFileInfo;
		}

		private Stream OpenStream( int startingPosition )
		{
			return streamFileInfo.OpenStream( startingPosition );
		}

		private StreamReader OpenReader( Stream stream )
		{
			return new StreamReader( stream, encoding );
		}

		private List<LogEntry> NoLogEntries()
		{
			return new List<LogEntry>();
		}

		private bool WasLineBreakAtTheEnd( string str )
		{
			if ( String.IsNullOrEmpty( str ) )
				return false;

			char lastChar = str[str.Length - 1];
			return lastChar == '\r' || lastChar == '\n';
		}

		#region ILogFileReader Members

		private string ReadAddedText()
		{
			string addedText = null;
			using ( Stream fs = OpenStream( (int)lastLineBreakByteIndex ) )
			{
				// todo use ReadTimeout

				// длина файла уменьшилась - пока рассматриваем только append
				if ( fs.Length < prevStreamLength )
				{
					logger.WriteError( "LogFile.OnFileChanged: Длина файла \"{0}\" уменьшилась с {1} до {2}", Name, prevStreamLength, fs.Length );
					return null;
				}

				if ( fs.Length == prevStreamLength )
				{
					// длина файла не изменилась
					logger.DebugWriteInfo( "LogFile.OnFileChanged: Длина файла \"{0}\" не изменилась ({1} байт)", Name, prevStreamLength );
					return null;
				}

				long streamLength = fs.Length;

				// todo асинхронное чтение тут и в ReadFile

				// начинаем читать с начала последней строки
				fs.Position = this.lastLineBreakByteIndex;
				using ( StreamReader reader = OpenReader( fs ) )
				{
					addedText = reader.ReadToEnd();
					bool wasLineBreakAtTheEnd = WasLineBreakAtTheEnd( addedText );

					if ( wasLineBreakAtTheEnd )
					{
						this.lastLineBreakByteIndex = streamLength;
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

							int bytesCountFromLastLineBreakToTheEndOfAddedText = encoding.GetByteCount( chars );

							this.lastLineBreakByteIndex += (addedText.Length - bytesCountFromLastLineBreakToTheEndOfAddedText);
						}
					}
				}

				this.prevStreamLength = streamLength;
			}

			return addedText;
		}

		public override IList<LogEntry> ReadToEnd( LogEntry lastAddedEntry )
		{
			string addedText = ReadAddedText();

			if ( String.IsNullOrEmpty( addedText ) )
			{
				logger.DebugWriteInfo( "LogFile.OnFileChanged: Файл \"{0}\": добавленный текст пуст.", this.Name );
				return NoLogEntries();
			}

			long lineIndex = Math.Max( this.linesCount - 1, 0 );

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

				linesCount = (int)(lineIndex + 1);
				lineIndex++;
				lastLineWasEmpty = emptyLine;
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
		/// <para/>Если длина строки больше, то считается, что выбрана неправильная кодировка, и бросаетмя исключение InvalidEncodingException.
		/// </summary>
		private const int MaxLineLength = 100000;

		public override IList<LogEntry> ReadEntireFile()
		{
			List<LogEntry> logEntries = new List<LogEntry>();

			using ( Stream stream = OpenStream( 0 ) )
			{
				int notificationsCount = 0;
				long lineIndex = 0;

				int length = (int)stream.Length;
				if ( length == 0 )
					return logEntries;

				using ( StreamReader reader = OpenReader( stream ) )
				{
					string line;
					string prevLine = String.Empty;
					long prevLineBreakIndex = 0;
					int notParsedLinesCount = 0;
					int bytesReadDelta = 0;
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
							throw new InvalidEncodingException( encoding, Name );
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
						int byteCount = encoding.GetByteCount( prevLine );
						prevLineBreakIndex = length - byteCount;
					}

					this.lastLineBreakByteIndex = prevLineBreakIndex;
				}

				linesCount = (int)lineIndex;

				if ( lastLineBreakByteIndex == length )
				{
					linesCount++;
					lastLineWasEmpty = true;
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
			int tid = -1;
			DateTime time = DateTime.MinValue;
			string lineText = null;

			bool isLastLineChanged = lineIndex == (linesCount - 1) && linesCount > 0 && !lastLineWasEmpty;

			// обновилась последняя строка файла?
			if ( isLastLineChanged )
			{
				// в последней строке было начало LogEntry?
				if ( LogLineParser.TryExtractLogEntryData( line, out type, out tid, out time, out lineText ) )
				{
					LogEntry lastEntry = lastCreatedEntry;
					// последняя запись состоит только из заголовка?
					if ( lastEntry.LinesCount == 1 )
					{
						lastEntry.UpdateHeader( type, tid, time, lineText );
						logger.DebugWriteVerbose( "LogFile.AppendLine: File = \"{0}\": updated header in line #{1}", Name, lineIndex );
					}
					else // неполный заголовок новой записи был ошибочно дописан в текст предыдущей записи -> исправляем
					{
						lastEntry.TextLines.RemoveLastItem();
						lastEntry.Freeze();

						// после удаления последней строки самая последняя запись перестала пропускаться фильтром?
						if ( !globalEntriesFilter.Include( lastEntry ) )
						{
							RemoveLastEntry( addedEntries );
						}

						lastCreatedEntry = new LogEntry( type, tid, time, lineText, (int)lineIndex, parentLogFile );
						TryAddEntry( lastCreatedEntry, addedEntries );
					}
				}
				else
				{
					latestAddedEntry.ReplaceLastLine( line, lineIndex );

					logger.DebugWriteVerbose( "LogFile.AppendLine: File = \"{0}\": replaced line #{1}", Name, lineIndex );
				}
			}
			else // добавилась новая строка
			{
				// в добавленной строке было начало нового LogEntry?
				if ( LogLineParser.TryExtractLogEntryData( line, out type, out tid, out time, out lineText ) )
				{
					lastCreatedEntry = new LogEntry( type, tid, time, lineText, (int)lineIndex, parentLogFile );

					LogEntryAppendResult result = TryAddEntry( lastCreatedEntry, addedEntries );
					if ( result == LogEntryAppendResult.Added )
					{
						// морозим предыдущий, если он был
						if ( latestAddedEntry != null )
							latestAddedEntry.Freeze();
					}
				}
				else
				{
					// добавляем к предыдущей записи лога
					if ( lastCreatedEntry != null )
					{
						lastCreatedEntry.AppendLine( line );

						bool wasNotAddedToList = lastCreatedEntry != latestAddedEntry;
						if ( wasNotAddedToList )
						{
							LogEntryAppendResult res = TryAddEntry( lastCreatedEntry, addedEntries );
							if ( res == LogEntryAppendResult.Added )
							{
								logger.DebugWriteVerbose( "LogFile.AppendLine: File = \"{0}\": Appended line #{1} with text \"{2}\"", Name, lineIndex, line );
							}
							return res;
						}
					}

					// нет последней линии - например, мы читаем файл и все линии до этого не проходили по дате
					// или неправильная кодировка - тогда в файл не будет добавлена ни одна запись
					if ( lastCreatedEntry == null )
						return LogEntryAppendResult.NotParsed;
				}
			}

			return LogEntryAppendResult.Added;
		}

		private LogEntryAppendResult TryAddEntry( LogEntry newEntry, IList<LogEntry> addedEntries )
		{
			if ( globalEntriesFilter.Include( newEntry ) )
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
