using System;
using System.Collections.Generic;
using System.IO;
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
	public sealed class LogFile : INotifyPropertyChanged, IReportReadProgress, ILogFile, ILogVisitable
	{
		private readonly Logger _logger;
		private readonly LogDirectory _parentDirectory;
		private readonly Encoding _encoding;
		public Encoding Encoding
		{
			get { return _encoding; }
		}

		private readonly IFileInfo _fileInfo;
		public IFileInfo FileInfo
		{
			get { return _fileInfo; }
		}

		private readonly LogFileReaderBase _logFileReader;
		public LogFileReaderBase LogFileReader
		{
			get { return _logFileReader; }
		}

		public string Name { get; private set; }
		public string FullPath { get; private set; }

		private int _linesCount;
		public int LinesCount
		{
			get { return _linesCount; }
		}

		private readonly IList<LogEntry> _logEntries = CollectionHelper.CreateList<LogEntry>();

		private readonly ObservableList<LogEntry> _entries;
		public ObservableList<LogEntry> LogEntries
		{
			get { return _entries; }
		}

		// todo brinchuk сделать еще метод, возвращающий навигатор с определенного смещения в файле
		public IBidirectionalEnumerable<LogEntry> GetNavigator()
		{
			return new LogFileNavigator( _fileInfo, new LogFileReaderArguments( ParentDirectory, this ) );
		}

		public LogDirectory ParentDirectory
		{
			get { return _parentDirectory; }
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

			_parentDirectory = parentDirectory;
			_logger = this._parentDirectory.Config.Logger;
			_encoding = this._parentDirectory.Encoding;

			_entries = new ObservableList<LogEntry>( _logEntries );

			_fileInfo = fileInfo;
			Name = fileInfo.Name;
			FullPath = fileInfo.FullName;

			_logFileReader = fileInfo.GetReader( new LogFileReaderArguments( parentDirectory, this ) );

			_logFileReader.FileReadProgress += OnLogFileReaderFileReadProgress;
		}

		private void OnLogFileReaderFileReadProgress( object sender, FileReadEventArgs e )
		{
			ReadProgress.Raise( this, e );
		}

		public void ReadFile()
		{
			IList<LogEntry> addedEntries = _logFileReader.ReadEntireFile();
			ProcessAddedEntries( addedEntries, 0 );
		}

		private void ProcessAddedEntries( IList<LogEntry> addedEntries, int startingIndex )
		{
			this._linesCount = addedEntries.Sum( e => e.LinesCount );

			_logEntries.AddRange( addedEntries );

			_entries.RaiseGenericCollectionItemsAdded( addedEntries, startingIndex );
			_parentDirectory.OnLogEntriesAddedToFile( addedEntries );

			PropertyChanged.RaiseAllChanged( this );
		}

		internal void OnFileChanged()
		{
			int startingIndex = _entries.Count;
			IList<LogEntry> addedLogEntries = _logFileReader.ReadToEnd( _logEntries.LastOrDefault() );

			ProcessAddedEntries( addedLogEntries, startingIndex );
		}

		public event EventHandler<FileReadEventArgs> ReadProgress;

		public long TotalLengthInBytes
		{
			get { return FileInfo.Length; }
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void Accept( ILogVisitor visitor )
		{
			visitor.Visit( this );
		}
	}
}
