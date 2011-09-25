using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Diagnostics;
using LogAnalyzer.Extensions;
using System.ComponentModel;
using LogAnalyzer.Collections;
using LogAnalyzer.Filters;
using System.Threading;
using LogAnalyzer.Kernel;

namespace LogAnalyzer
{
	// todo probably override GetHashCode() and Equals() methods.
	[DebuggerDisplay( "LogFile {FullPath}" )]
	public sealed class LogFile : INotifyPropertyChanged, IReportReadProgress
	{
		// todo brinchuk remove 3 of me
		private const string LogLineRegexText = @"^\[(?<Type>.)] \[(?<TID>.{3,4})] (?<Time>\d{2}\.\d{2}\.\d{4} \d{1,2}:\d{2}:\d{2})\t(?<Text>.*)$";
		private static readonly Regex logLineRegex = new Regex( LogLineRegexText, RegexOptions.Compiled );
		public static readonly string DateTimeFormat = "dd.MM.yyyy H:mm:ss";

		private readonly IFilter<LogEntry> globalEntriesFilter;
		private readonly Logger logger;
		private readonly LogDirectory parentDirectory;
		private readonly Encoding encoding;
		public Encoding Encoding
		{
			get { return encoding; }
		}

		public IFileInfo FileInfo { get; private set; }
		public string Name { get; private set; }
		public string FullPath { get; private set; }

		private long prevStreamLength;
		private long lastLineBreakByteIndex;

		private int linesCount;
		public int LinesCount
		{
			get { return linesCount; }
		}

		private readonly IList<LogEntry> logEntries = CollectionHelper.CreateList<LogEntry>();

		private readonly ThinListWrapper<LogEntry> entriesWrapper;
		public ThinListWrapper<LogEntry> LogEntries
		{
			get { return entriesWrapper; }
		}

		public LogDirectory ParentDirectory
		{
			get
			{
				return parentDirectory;
			}
		}

		private bool lastLineWasEmpty;

		internal LogFile( IFileInfo fileInfo, LogDirectory parent )
		{
			entriesWrapper = null;
			if ( fileInfo == null )
				throw new ArgumentNullException( "fileInfo" );
			if ( parent == null )
				throw new ArgumentNullException( "parent" );

			parentDirectory = parent;
			logger = parentDirectory.Config.Logger;
			encoding = parentDirectory.Encoding;
			globalEntriesFilter = parent.GlobalEntriesFilter;

			entriesWrapper = new ThinListWrapper<LogEntry>( logEntries );

			FileInfo = fileInfo;
			Name = fileInfo.Name;
			FullPath = fileInfo.FullName;

			long length = fileInfo.Length;
			prevStreamLength = length;
		}

		private Stream OpenStream( int startPosition )
		{
			throw new NotImplementedException();
			//return FileInfo.OpenStream( startPosition );
		}

		// todo brinchuk remove me
		private StreamReader OpenReader( Stream stream )
		{
			return new StreamReader( stream, encoding );
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
		private const int MaxLineLength = 40000;

		public void ReadFile()
		{
			long length = FileInfo.Length;
			if ( length == 0 )
				return;

			using ( Stream stream = OpenStream( 0 ) )
			{
				int notificationsCount = 0;
				long lineIndex = 0;
				using ( StreamReader reader = OpenReader( stream ) )
				{
					string line;
					string prevLine = String.Empty;
					long prevLineBreakIndex = 0;
					int notParsedLinesCount = 0;
					int bytesReadDelta = 0;
					while ( (line = reader.ReadLine()) != null )
					{
						LogEntryAppendResult lineAppendResult = AppendLine( line, lineIndex );

						if ( (notificationsCount + 1) * FileReadNotificationBytesStep < stream.Position )
						{
							bytesReadDelta = (int)(stream.Position - notificationsCount * FileReadNotificationBytesStep);

							ReadProgress.Raise( this, new FileReadEventArgs { BytesReadSincePreviousCall = bytesReadDelta } );
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
							throw new InvalidEncodingException( this );
						}
					}

					bytesReadDelta = (int)(stream.Position - notificationsCount * FileReadNotificationBytesStep);
					ReadProgress.Raise( this, new FileReadEventArgs { BytesReadSincePreviousCall = bytesReadDelta } );

					bool wasLineBreak = WasLineBreakAtTheEnd( prevLine );
					if ( wasLineBreak )
					{
						prevLineBreakIndex = length;
					}
					else
					{
						// если последняя считанная строка не заканчивается переносом строки,
						// то считаем, что перенос строки был перед началом последней строки.
						int byteCount = parentDirectory.Encoding.GetByteCount( prevLine );
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

			entriesWrapper.RaiseCollectionAdded( addedEntries );
			parentDirectory.OnLogEntriesAddedToFile( addedEntries );

			PropertyChanged.RaiseAllChanged( this );
		}

		// todo brinchuk remove me
		private static bool WasLineBreakAtTheEnd( string str )
		{
			if ( String.IsNullOrEmpty( str ) )
				return false;

			char lastChar = str[str.Length - 1];
			return lastChar == '\r' || lastChar == '\n';
		}

		private LogEntry lastCreatedEntry;

		// todo brinchuk remove me
		/// <summary>
		/// 
		/// </summary>
		/// <param name="line"></param>
		/// <param name="lineIndex"></param>
		/// <returns>Была ли на самом деле добавлена новая строка?</returns>
		private LogEntryAppendResult AppendLine( string line, long lineIndex )
		{
			if ( line == null )
				throw new ArgumentNullException( "line" );

			// некорректные значения
			string type = null;
			int tid = -1;
			DateTime time = DateTime.MinValue;
			string lineText = null;

			bool isLastLineChanged = lineIndex == (LinesCount - 1) && LinesCount > 0 && !lastLineWasEmpty;

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
							RemoveLastEntry();
						}

						lastCreatedEntry = new LogEntry( type, tid, time, lineText, (int)lineIndex, this );
						TryAddEntry( lastCreatedEntry );
					}
				}
				else
				{
					LogEntry entry = GetLastAddedLogEntry();
					entry.ReplaceLastLine( line, lineIndex );

					logger.DebugWriteVerbose( "LogFile.AppendLine: File = \"{0}\": replaced line #{1}", Name, lineIndex );
				}
			}
			else // добавилась новая строка
			{
				// в добавленной строке было начало нового LogEntry?
				if ( TryExtractLogEntryData( line, out type, out tid, out time, out lineText ) )
				{
					lastCreatedEntry = new LogEntry( type, tid, time, lineText, (int)lineIndex, this );

					LogEntry lastAddedEntry = GetLastAddedLogEntry();

					LogEntryAppendResult result = TryAddEntry( lastCreatedEntry );
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

						bool wasNotAddedToList = lastCreatedEntry != GetLastAddedLogEntry();
						if ( wasNotAddedToList )
						{
							LogEntryAppendResult res = TryAddEntry( lastCreatedEntry );
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

		private LogEntryAppendResult TryAddEntry( LogEntry newEntry )
		{
			if ( globalEntriesFilter.Include( newEntry ) )
			{
				logEntries.Add( newEntry );
				addedEntries.Add( newEntry );

				return LogEntryAppendResult.Added;
			}
			else
			{
				return LogEntryAppendResult.ExcludedByFilter;
			}
		}

		private void RemoveLastEntry()
		{
			logEntries.RemoveLastItem();
			addedEntries.RemoveLastItem();
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

		private LogEntry GetLastAddedLogEntry()
		{
			LogEntry logEntry = logEntries.LastOrDefault();
			return logEntry;
		}

		private List<LogEntry> addedEntries = new List<LogEntry>();

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

		internal void OnFileChanged()
		{
			string addedText = ReadAddedText();

			if ( String.IsNullOrEmpty( addedText ) )
			{
				logger.DebugWriteInfo( "LogFile.OnFileChanged: Файл \"{0}\": добавленный текст пуст.", this.Name );
				return;
			}

			long lineIndex = Math.Max( this.linesCount - 1, 0 );

			string[] addedLines = addedText.Split( new[] { Environment.NewLine }, StringSplitOptions.None );

			addedEntries = new List<LogEntry>();

			foreach ( var line in addedLines )
			{
				Condition.DebugAssert( !(line.Contains( '\r' ) || line.Contains( '\n' )), "Added text contains new line characters." );

				bool emptyLine = String.IsNullOrEmpty( line );
				if ( !emptyLine )
				{
					LogEntryAppendResult _ = this.AppendLine( line, lineIndex );
				}

				linesCount = (int)(lineIndex + 1);
				lineIndex++;
				lastLineWasEmpty = emptyLine;
			}

			entriesWrapper.RaiseCollectionAdded( addedEntries );
			parentDirectory.OnLogEntriesAddedToFile( addedEntries );

			PropertyChanged.Raise( this, "LinesCount" );
		}

		public event EventHandler<FileReadEventArgs> ReadProgress;

		public int TotalLengthInBytes
		{
			get { return (int)FileInfo.Length; }
		}


		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

	}

	public sealed class FileReadEventArgs : EventArgs
	{
		public int BytesReadSincePreviousCall { get; set; }
	}
}
