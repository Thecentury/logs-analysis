﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LogAnalyzer.Collections;
using LogAnalyzer.Extensions;
using System.Diagnostics;
using System.ComponentModel;
using LogAnalyzer.Kernel;
using LogAnalyzer.Logging;

namespace LogAnalyzer
{
	public abstract class LogEntriesList : INotifyPropertyChanged, IReportReadProgress, ISaveable
	{
		private readonly CompositeObservableListWrapper<LogEntry> mergedEntriesWrapper;
		public CompositeObservableListWrapper<LogEntry> MergedEntries
		{
			get { return mergedEntriesWrapper; }
		}

		private readonly LogEntrySortedCollection logEntrySortedCollection;

		private List<LogEntry> mergedEntriesList;
		protected internal List<LogEntry> MergedEntriesList
		{
			get { return mergedEntriesList; }
			protected set { mergedEntriesList = value; }
		}

		private readonly IEnvironment environment;
		public IEnvironment Environment
		{
			get { return environment; }
		}

		protected readonly Logger logger;

		protected LogEntriesList( IEnvironment environment, Logger logger )
		{
			if ( environment == null )
				throw new ArgumentNullException( "environment" );
			if ( logger == null )
				throw new ArgumentNullException( "logger" );

			this.logger = logger;
			this.environment = environment;

			mergedEntriesList = new List<LogEntry>();
			logEntrySortedCollection = new LogEntrySortedCollection( this, environment );
			mergedEntriesWrapper = new CompositeObservableListWrapper<LogEntry>( mergedEntriesList, logEntrySortedCollection.UnappendedLogEntries );
		}

		internal void EnqueueToMerge( IList<LogEntry> addedEntries )
		{
			int startingIndex = MergedEntries.Count;

			logEntrySortedCollection.Enqueue( addedEntries );

			MergedEntries.RaiseGenericCollectionItemsAdded( addedEntries, startingIndex );

			messageSeverityCount.Update( addedEntries );
		}

		private readonly MessageSeverityCount messageSeverityCount = new MessageSeverityCount();
		public MessageSeverityCount MessageSeverityCount
		{
			get { return messageSeverityCount; }
		}

		private DateTime loadStartTime;

		private bool isLoaded;
		public bool IsLoaded
		{
			get { return isLoaded; }
		}

		public event EventHandler Loaded;

		private bool haveStarted;
		public bool HaveStarted
		{
			get { return haveStarted; }
		}

		[DebuggerStepThrough]
		public void Start()
		{
			haveStarted = true;
			loadStartTime = DateTime.Now;
			StartImpl();
		}

		protected abstract void StartImpl();

		protected void RaiseLoadedEvent()
		{
			isLoaded = true;
			Loaded.Raise( this );
			RaiseAllPropertiesChanged();

			TimeSpan loadingDuration = DateTime.Now - loadStartTime;

			logger.WriteInfo( "{0}: loaded in {1} seconds", ToString(), loadingDuration.TotalSeconds );

			// не держим ссылки на подписчиков, 
			// все равно это событие больше вызвано не будет
			ReadProgress = null;
		}

		internal void AppendLogEntryToMergedList( LogEntry logEntry )
		{
			logger.DebugWriteVerbose( "{0}.AppendMergedLogEntry: +\"{1}\"", GetType().Name, logEntry.TextLines.FirstOrDefault() );
			mergedEntriesList.Add( logEntry );
			// todo notify about collection change
		}

		// todo возможно, кидать исключение в Release при попытке подписаться
		/// <summary>
		/// Событие возникает, когда в общий список добавляется запись лога.
		/// <para/>
		/// Пока предполагается, что это событие только для DEBUG-версии.
		/// </summary>
		internal event EventHandler<LogEntryAddedEventArgs> LogEntryAdded;

		[Conditional( "DEBUG" )]
		internal void RaiseLogEntryAdded( IEnumerable<LogEntry> logEntries )
		{
			var logEntryAdded = LogEntryAdded;
			if ( logEntryAdded != null )
			{
				foreach ( LogEntry logEntry in logEntries )
				{
					logEntryAdded( this, new LogEntryAddedEventArgs( logEntry ) );
				}
			}
		}

		internal abstract int TotalFilesCount
		{
			get;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void RaisePropertyChanged( string propertyName )
		{
			PropertyChanged.Raise( this, propertyName );
		}

		protected void RaiseAllPropertiesChanged()
		{
			PropertyChanged.RaiseAllChanged( this );
		}

		#region IReportReadProgress Members

		public event EventHandler<FileReadEventArgs> ReadProgress;
		protected void RaiseReadProgress( FileReadEventArgs e )
		{
			ReadProgress.Raise( this, e );
		}

		public abstract long TotalLengthInBytes
		{
			get;
		}

		#endregion

		public void Write( TextWriter writer )
		{
			foreach ( var logEntry in mergedEntriesWrapper )
			{
				logEntry.Write( writer );
			}
		}
	}
}
