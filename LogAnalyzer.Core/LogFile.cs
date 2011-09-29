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
	[DebuggerDisplay( "LogFile {FullPath}" )]
	public sealed class LogFile : INotifyPropertyChanged, IReportReadProgress
	{
		public static readonly string DateTimeFormat = "dd.MM.yyyy H:mm:ss";

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

		private readonly ThinListWrapper<LogEntry> entriesWrapper;
		public ThinListWrapper<LogEntry> LogEntries
		{
			get { return entriesWrapper; }
		}

		public LogDirectory ParentDirectory
		{
			get { return parentDirectory; }
		}

		internal LogFile( IFileInfo fileInfo, LogDirectory parent )
		{
			if ( fileInfo == null )
				throw new ArgumentNullException( "fileInfo" );
			if ( parent == null )
				throw new ArgumentNullException( "parent" );

			parentDirectory = parent;
			logger = parentDirectory.Config.Logger;
			encoding = parentDirectory.Encoding;

			entriesWrapper = new ThinListWrapper<LogEntry>( logEntries );

			this.fileInfo = fileInfo;
			Name = fileInfo.Name;
			FullPath = fileInfo.FullName;

			this.logFileReader = fileInfo.GetReader(
				new LogFileReaderArguments
				{
					Encoding = encoding,
					Logger = logger,
					ParentLogFile = this,
					GlobalEntriesFilter = parent.GlobalEntriesFilter
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
			ProcessAddedEntries( addedEntries );
		}

		private void ProcessAddedEntries( IList<LogEntry> addedEntries )
		{
			linesCount = addedEntries.Sum( e => e.LinesCount );

			logEntries.AddRange( addedEntries );

			entriesWrapper.RaiseCollectionAdded( addedEntries );
			parentDirectory.OnLogEntriesAddedToFile( addedEntries );

			PropertyChanged.RaiseAllChanged( this );
		}

		internal void OnFileChanged()
		{
			IList<LogEntry> addedLogEntries = logFileReader.ReadToEnd( logEntries.LastOrDefault() );

			ProcessAddedEntries( addedLogEntries );
		}

		public event EventHandler<FileReadEventArgs> ReadProgress;

		public int TotalLengthInBytes
		{
			get { return FileInfo.Length; }
		}

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion
	}
}
