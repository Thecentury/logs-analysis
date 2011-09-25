using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LogAnalyzer.Extensions;
using LogAnalyzer.Filters;

namespace LogAnalyzer
{
	public interface ILogFileReader
	{
		IList<LogEntry> ReadFromCurrentPositionToTheEnd( IList<LogEntry> exisingEntries );
	}

	internal sealed class StreamLogFileReader : ILogFileReader
	{
		private const string LogLineRegexText = @"^\[(?<Type>.)] \[(?<TID>.{3,4})] (?<Time>\d{2}\.\d{2}\.\d{4} \d{1,2}:\d{2}:\d{2})\t(?<Text>.*)$";

		private static readonly Regex logLineRegex = new Regex( LogLineRegexText, RegexOptions.Compiled );
		public static readonly string DateTimeFormat = "dd.MM.yyyy H:mm:ss";

		private readonly IFilter<LogEntry> globalEntriesFilter = null;

		private long lastLineBreakByteIndex;
		private long prevStreamLength;
		private readonly Logger logger;
		private readonly Encoding encoding;
		private int linesCount = 0;
		private bool lastLineWasEmpty = false;
		private LogEntry lastCreatedEntry = null;
		private readonly LogFile parentLogFile;

		private string Name
		{
			get { return parentLogFile.Name; }
		}

		public StreamLogFileReader( Logger logger, Encoding encoding, LogFile parentLogLogFile )
		{
			if ( logger == null )
				throw new ArgumentNullException( "logger" );
			if ( encoding == null )
				throw new ArgumentNullException( "encoding" );
			if ( parentLogLogFile == null )
				throw new ArgumentNullException( "parentLogLogFile" );

			this.logger = logger;
			this.parentLogFile = parentLogLogFile;
			this.encoding = encoding;
		}

		private Stream OpenStream( int startingPosition )
		{
			throw new NotImplementedException();
		}

		private StreamReader OpenReader( Stream stream )
		{
			throw new NotImplementedException();
		}

		private List<LogEntry> NoLogEntries()
		{
			return new List<LogEntry>();
		}

		private bool WasLineBreakAtTheEnd( string str )
		{
			throw new NotImplementedException();
		}

		#region ILogFileReader Members

		private string ReadAddedText()
		{
			string addedText = null;
			using ( Stream fs = OpenStream( (int)lastLineBreakByteIndex ) )
			{
				// todo use ReadTimeout

				// длина файла уменьшилась - пока рассматриваем только append
				if ( fs.Length < this.prevStreamLength )
				{
					logger.WriteError( "LogFile.OnFileChanged: Длина файла \"{0}\" уменьшилась с {1} до {2}", this.Name, this.prevStreamLength, fs.Length );
					return addedText;
				}
				else if ( fs.Length == this.prevStreamLength )
				{
					// длина файла не изменилась
					logger.DebugWriteInfo( "LogFile.OnFileChanged: Длина файла \"{0}\" не изменилась ({1} байт)", this.Name, this.prevStreamLength );
					return addedText;
				}

				long streamLength = fs.Length;

				// todo асинхронное чтение тут и в ReadFile

				// начинаем читать с начала последней строки
				fs.Position = this.lastLineBreakByteIndex;
				bool wasLineBreakAtTheEnd = false;
				using ( StreamReader reader = OpenReader( fs ) )
				{
					addedText = reader.ReadToEnd();
					wasLineBreakAtTheEnd = WasLineBreakAtTheEnd( addedText );

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

		public IList<LogEntry> ReadFromCurrentPositionToTheEnd( IList<LogEntry> exisingEntries )
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
					LogEntryAppendResult _ = this.AppendLine( line, lineIndex, exisingEntries, addedEntries );
				}

				linesCount = (int)(lineIndex + 1);
				lineIndex++;
				lastLineWasEmpty = emptyLine;
			}

			return addedEntries;
		}

		private bool TryExtractLogEntryData( string line, out string type, out int tid, out DateTime time, out string text )
		{
			// init
			type = null;
			tid = -1;
			time = DateTime.MinValue;
			text = null;

			Match match = logLineRegex.Match( line );
			if ( match.Success )
			{
				// выдираем данные
				type = match.Groups["Type"].Value;
				if ( String.IsNullOrWhiteSpace( type ) || type.Length > 1 )
				{
					// todo ошибка!!!
					throw new InvalidOperationException();
				}

				string tidStr = match.Groups["TID"].Value;
				tid = 0;
				if ( !Int32.TryParse( tidStr, out tid ) )
				{
					// todo ошибка!!!
					throw new InvalidOperationException();
				}

				string timeStr = match.Groups["Time"].Value;
				time = DateTime.MinValue;
				if ( !DateTime.TryParseExact( timeStr, DateTimeFormat, null, DateTimeStyles.None, out time ) )
				{
					// todo error!
					throw new InvalidOperationException();
				}

				text = match.Groups["Text"].Value;

				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="line"></param>
		/// <param name="lineIndex"></param>
		/// <param name="existingEntries"></param>
		/// <param name="addedEntries"></param>
		/// <returns>Была ли на самом деле добавлена новая строка?</returns>
		private LogEntryAppendResult AppendLine( string line, long lineIndex, IList<LogEntry> existingEntries, IList<LogEntry> addedEntries )
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
				if ( TryExtractLogEntryData( line, out type, out tid, out time, out lineText ) )
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
					LogEntry entry = GetLastAddedLogEntry( existingEntries );
					entry.ReplaceLastLine( line, lineIndex );

					logger.DebugWriteVerbose( "LogFile.AppendLine: File = \"{0}\": replaced line #{1}", Name, lineIndex );
				}
			}
			else // добавилась новая строка
			{
				// в добавленной строке было начало нового LogEntry?
				if ( TryExtractLogEntryData( line, out type, out tid, out time, out lineText ) )
				{
					lastCreatedEntry = new LogEntry( type, tid, time, lineText, (int)lineIndex, parentLogFile );

					LogEntry lastAddedEntry = GetLastAddedLogEntry( existingEntries );

					LogEntryAppendResult result = TryAddEntry( lastCreatedEntry, addedEntries );
					if ( result == LogEntryAppendResult.Added )
					{
						// морозим предыдущий, если он был
						if ( lastAddedEntry != null )
							lastAddedEntry.Freeze();
					}
				}
				else
				{
					// добавляем к предыдущей записи лога
					if ( lastCreatedEntry != null )
					{
						lastCreatedEntry.AppendLine( line );

						bool wasNotAddedToList = lastCreatedEntry != GetLastAddedLogEntry( existingEntries );
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

		private LogEntry GetLastAddedLogEntry( IList<LogEntry> logEntries )
		{
			LogEntry logEntry = logEntries.LastOrDefault();
			return logEntry;
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
