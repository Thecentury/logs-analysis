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

		private readonly IFileInfo fileInfo;
		public IFileInfo FileInfo
		{
			get { return fileInfo; }
		}

		private readonly LogFileReaderBase logFileReader;
		public LogFileReaderBase LogFileReader
		{
			get { return logFileReader; }
		}

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
			if ( fileInfo == null )
				throw new ArgumentNullException( "fileInfo" );
			if ( parent == null )
				throw new ArgumentNullException( "parent" );

			parentDirectory = parent;
			logger = parentDirectory.Config.Logger;
			encoding = parentDirectory.Encoding;
			globalEntriesFilter = parent.GlobalEntriesFilter;

			entriesWrapper = new ThinListWrapper<LogEntry>( logEntries );

			this.fileInfo = fileInfo;
			Name = fileInfo.Name;
			FullPath = fileInfo.FullName;

			this.logFileReader = fileInfo.GetReader( new LogFileReaderArguments { Encoding = encoding, Logger = logger, ParentLogFile = this } );

			long length = fileInfo.Length;
			prevStreamLength = length;
		}

		[Obsolete]
		private Stream OpenStream( int startPosition )
		{
			throw new NotImplementedException();
			//return FileInfo.OpenStream( startPosition );
		}

		// todo brinchuk remove me
		[Obsolete]
		private StreamReader OpenReader( Stream stream )
		{
			return new StreamReader( stream, encoding );
		}


		public void ReadFile()
		{
			IList<LogEntry> addedEntries = logFileReader.ReadEntireFile();

			entriesWrapper.RaiseCollectionAdded( addedEntries );
			parentDirectory.OnLogEntriesAddedToFile( addedEntries );

			PropertyChanged.RaiseAllChanged( this );
		}

		internal void OnFileChanged()
		{
			IList<LogEntry> addedLogEntries = logFileReader.ReadToEnd( logEntries.LastOrDefault() );

			entriesWrapper.RaiseCollectionAdded( addedLogEntries );
			parentDirectory.OnLogEntriesAddedToFile( addedLogEntries );

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
