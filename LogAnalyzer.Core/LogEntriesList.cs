using System;
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
	public abstract class LogEntriesList : INotifyPropertyChanged, IReportReadProgress
	{
		private readonly CompositeObservableListWrapper<LogEntry> _mergedEntriesWrapper;
		public CompositeObservableListWrapper<LogEntry> MergedEntries
		{
			get { return _mergedEntriesWrapper; }
		}

		private readonly LogEntrySortedCollection _logEntrySortedCollection;

		private List<LogEntry> _mergedEntriesList;
		protected internal List<LogEntry> MergedEntriesList
		{
			get { return _mergedEntriesList; }
			protected set { _mergedEntriesList = value; }
		}

		private readonly IEnvironment _environment;
		public IEnvironment Environment
		{
			get { return _environment; }
		}

		protected readonly Logger Logger;

		protected LogEntriesList( IEnvironment environment, Logger logger )
		{
			if ( environment == null )
			{
				throw new ArgumentNullException( "environment" );
			}
			if ( logger == null )
			{
				throw new ArgumentNullException( "logger" );
			}

			this.Logger = logger;
			this._environment = environment;

			_mergedEntriesList = new List<LogEntry>();
			_logEntrySortedCollection = new LogEntrySortedCollection( this, environment );
			_mergedEntriesWrapper = new CompositeObservableListWrapper<LogEntry>( _mergedEntriesList, _logEntrySortedCollection.UnappendedLogEntries );
		}

		internal void EnqueueToMerge( IList<LogEntry> addedEntries )
		{
			int startingIndex = MergedEntries.Count;

			_logEntrySortedCollection.Enqueue( addedEntries );

			MergedEntries.RaiseGenericCollectionItemsAdded( addedEntries, startingIndex );

			_messageSeverityCount.Update( addedEntries );
		}

		private readonly MessageSeverityCount _messageSeverityCount = new MessageSeverityCount();
		public MessageSeverityCount MessageSeverityCount
		{
			get { return _messageSeverityCount; }
		}

		private DateTime _loadStartTime;

		private bool _isLoaded;
		public bool IsLoaded
		{
			get { return _isLoaded; }
		}

		public event EventHandler Loaded;

		private bool _haveStarted;
		public bool HaveStarted
		{
			get { return _haveStarted; }
		}

		[DebuggerStepThrough]
		public void Start()
		{
			_haveStarted = true;
			_loadStartTime = DateTime.Now;
			StartImpl();
		}

		protected abstract void StartImpl();

		protected void RaiseLoadedEvent()
		{
			_isLoaded = true;
			Loaded.Raise( this );
			RaiseAllPropertiesChanged();

			TimeSpan loadingDuration = DateTime.Now - _loadStartTime;

			Logger.WriteInfo( "{0}: loaded in {1} seconds", ToString(), loadingDuration.TotalSeconds );

			// не держим ссылки на подписчиков, 
			// все равно это событие больше вызвано не будет
			ReadProgress = null;
		}

		internal void AppendLogEntryToMergedList( LogEntry logEntry )
		{
			Logger.DebugWriteVerbose( "{0}.AppendMergedLogEntry: +\"{1}\"", GetType().Name, logEntry.TextLines.FirstOrDefault() );
			_mergedEntriesList.Add( logEntry );
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

		protected void PerformInitialMerge( int capacity, IEnumerable<LogEntry> entries )
		{
			MergedEntriesList.Capacity = capacity;

			Stopwatch timer = Stopwatch.StartNew();

#if true
			_mergedEntriesList.AddRange( entries
										.AsParallel()
										.OrderBy( LogEntryByDateAndIndexComparer.Instance ) );
#else
			_mergedEntriesList.AddRange( entries );
			ParallelSort.QuicksortParallel( _mergedEntriesList, LogEntryByDateAndIndexComparer.Instance );
#endif

			timer.Stop();
			Logger.WriteInfo( "{1}: Sorting - elapsed {0} ms", timer.ElapsedMilliseconds, GetType().Name );

#if ASSERT
			Condition.DebugAssert( sortedEntries.AreSorted( LogEntryByDateComparer.Instance ) );
#endif

			MergedEntries.RaiseCollectionReset();
			MessageSeverityCount.Update( _mergedEntriesList );
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
	}
}
