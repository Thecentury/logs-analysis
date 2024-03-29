﻿using System;
using System.Collections;
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
	public sealed class LogFile : INotifyPropertyChanged, IReportReadProgress, ILogFile, ILogVisitable, INavigatable<LogEntry>
	{
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

		public string CleanedName
		{
			get
			{
				string cleanName = LogFileNameCleaner.GetCleanedName( Name );
				return cleanName;
			}
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

		private IBidirectionalEnumerable<LogEntry> _fillingNavigator;
		public IBidirectionalEnumerable<LogEntry> GetFillingNavigator()
		{
			throw new NotImplementedException();
		}

		private sealed class FillingNavigator : IBidirectionalEnumerable<LogEntry>
		{
			private sealed class FillingEnumerator : IBidirectionalEnumerator<LogEntry>
			{
				public bool MoveBack()
				{
					throw new NotImplementedException();
				}

				public void Dispose()
				{
					throw new NotImplementedException();
				}

				public bool MoveNext()
				{
					throw new NotImplementedException();
				}

				public void Reset()
				{
					throw new NotImplementedException();
				}

				public LogEntry Current
				{
					get { throw new NotImplementedException(); }
				}

				object IEnumerator.Current
				{
					get { return Current; }
				}
			}

			public IBidirectionalEnumerator<LogEntry> GetEnumerator()
			{
				throw new NotImplementedException();
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

		public LogDirectory ParentDirectory
		{
			get { return _parentDirectory; }
		}

		/// <summary>
		/// Для тестов.
		/// </summary>
		private LogFile() { }

		/// <summary>
		/// Для тестов.
		/// </summary>
		public static LogFile CreateEmpty()
		{
			return new LogFile();
		}

		public static LogFile CreateEmpty( string name )
		{
			return new LogFile { Name = name };
		}

		public LogFile( IFileInfo fileInfo, LogDirectory parentDirectory = null )
		{
			if ( fileInfo == null )
			{
				throw new ArgumentNullException( "fileInfo" );
			}

			_parentDirectory = parentDirectory;
			if ( parentDirectory != null )
			{
				_encoding = this._parentDirectory.Encoding;
				_logFileReader = fileInfo.GetReader( new LogFileReaderArguments( parentDirectory, this ) );
				_logFileReader.FileReadProgress += OnLogFileReaderFileReadProgress;
			}

			_entries = new ObservableList<LogEntry>( _logEntries );

			_fileInfo = fileInfo;
			Name = fileInfo.Name;
			FullPath = fileInfo.FullName;
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
