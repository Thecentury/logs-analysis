using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using LogAnalyzer.Extensions;
using System.ComponentModel;
using LogAnalyzer.Collections;
using LogAnalyzer.Kernel;
using LogAnalyzer.Logging;

namespace LogAnalyzer
{
	[DebuggerDisplay( "LogFile {FullPath}" )]
	public sealed class LogFile : INotifyPropertyChanged, IReportReadProgress, ILogFile
	{
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


		private int linesCount;
		public int LinesCount
		{
			get { return linesCount; }
		}

		private readonly IList<LogEntry> logEntries = CollectionHelper.CreateList<LogEntry>();

		private readonly ObservableList<LogEntry> entries;
		public ObservableList<LogEntry> LogEntries
		{
			get { return entries; }
		}

		public LogDirectory ParentDirectory
		{
			get { return parentDirectory; }
		}

		/// <summary>
		/// Для тестов.
		/// </summary>
		internal LogFile() { }

		public LogFile( IFileInfo fileInfo, LogDirectory parentDirectory )
		{
			if ( fileInfo == null )
				throw new ArgumentNullException( "fileInfo" );
			if ( parentDirectory == null )
				throw new ArgumentNullException( "parentDirectory" );

			this.parentDirectory = parentDirectory;
			logger = this.parentDirectory.Config.Logger;
			encoding = this.parentDirectory.Encoding;

			entries = new ObservableList<LogEntry>( logEntries );

			this.fileInfo = fileInfo;
			Name = fileInfo.Name;
			FullPath = fileInfo.FullName;

			logFileReader = fileInfo.GetReader(
				new LogFileReaderArguments
				{
					Encoding = encoding,
					Logger = logger,
					ParentLogFile = this,
					GlobalEntriesFilter = parentDirectory.GlobalEntriesFilter,
					LineParser = parentDirectory.LineParser
				} );

			logFileReader.FileReadProgress += OnLogFileReaderFileReadProgress;
		}

		private void OnLogFileReaderFileReadProgress( object sender, FileReadEventArgs e )
		{
			ReadProgress.Raise( this, e );
		}

		public void ReadFile()
		{
			IList<LogEntry> addedEntries = logFileReader.ReadEntireFile();
			ProcessAddedEntries( addedEntries, 0 );
		}

		private void ProcessAddedEntries( IList<LogEntry> addedEntries, int startingIndex )
		{
			this.linesCount = addedEntries.Sum( e => e.LinesCount );

			logEntries.AddRange( addedEntries );

			entries.RaiseGenericCollectionItemsAdded( addedEntries, startingIndex );
			parentDirectory.OnLogEntriesAddedToFile( addedEntries );

			PropertyChanged.RaiseAllChanged( this );
		}

		internal void OnFileChanged()
		{
			int startingIndex = entries.Count;
			IList<LogEntry> addedLogEntries = logFileReader.ReadToEnd( logEntries.LastOrDefault() );

			ProcessAddedEntries( addedLogEntries, startingIndex );
		}

		public event EventHandler<FileReadEventArgs> ReadProgress;

		public long TotalLengthInBytes
		{
			get { return FileInfo.Length; }
		}

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion
	}
}
