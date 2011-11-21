﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using LogAnalyzer.Extensions;
using LogAnalyzer.Logging;

namespace LogAnalyzer.GUI.ViewModels.Collections
{
	/// <summary>
	/// Syncronized Collection + IList for LogEntryViewModels.
	/// </summary>
	[DebuggerDisplay( "SparseLogEntryViewModelList Count={Count}" )]
	public sealed class SparseLogEntryViewModelList : DispatcherObservableCollection, IList<LogEntryViewModel>, IList, ILogEntryHost
	{
		private readonly IList<LogEntry> logEntries;
		private readonly Func<LogEntry, LogFileViewModel> fileViewModelFactory;
		private readonly Dictionary<int, LogEntryViewModel> viewModelsCache = new Dictionary<int, LogEntryViewModel>();
		private readonly LogEntriesListViewModel parent;

		public LogEntriesListViewModel Parent
		{
			get { return parent; }
		}

		internal SparseLogEntryViewModelList( LogEntriesListViewModel parentViewModel, Func<LogEntry, LogFileViewModel> fileViewModelFactory )
			: base( parentViewModel.Entries, parentViewModel.Scheduler )
		{
			if ( parentViewModel == null )
				throw new ArgumentNullException( "parentViewModel" );
			if ( fileViewModelFactory == null )
				throw new ArgumentNullException( "fileViewModelFactory" );

			this.parent = parentViewModel;
			this.logEntries = parentViewModel.Entries;
			this.fileViewModelFactory = fileViewModelFactory;
		}

		internal ICollection<LogEntryViewModel> CreatedEntries
		{
			get { return viewModelsCache.Values; }
		}

		#region LogEntryHost Members

		void ILogEntryHost.Release( LogEntryViewModel vm )
		{
			viewModelsCache.Remove( vm.IndexInParentCollection );
			ItemRemoved.Raise( this, new LogEntryHostChangedEventArgs( vm ) );
		}

		public event EventHandler<LogEntryHostChangedEventArgs> ItemCreated;
		public event EventHandler<LogEntryHostChangedEventArgs> ItemRemoved;

		#endregion

		protected override void OnCollectionChanged( NotifyCollectionChangedEventArgs e )
		{
			switch ( e.Action )
			{
				case NotifyCollectionChangedAction.Add:
					ProcessItemsAdded( e );
					break;
				case NotifyCollectionChangedAction.Remove:
				case NotifyCollectionChangedAction.Replace:
				case NotifyCollectionChangedAction.Move:
					throw new NotSupportedException();
				case NotifyCollectionChangedAction.Reset:
					viewModelsCache.Clear();
					base.OnCollectionChanged( e );
					break;
			}
		}

		private void ProcessItemsAdded( NotifyCollectionChangedEventArgs e )
		{
			var addedLight = new List<LogEntryViewModel>( e.NewItems.Count );

			for ( int i = 0; i < e.NewItems.Count; i++ )
			{
				int index = e.NewStartingIndex + i;
				var addedItem = GetLogEntryViewModelByIndex(index, addTocache: false);
				addedLight.Add( addedItem );
			}

			base.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, addedLight, e.NewStartingIndex ) );
		}

		public LogEntryViewModel this[int index]
		{
			get
			{
				return GetLogEntryViewModelByIndex( index, addTocache: true );
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		private LogEntryViewModel GetLogEntryViewModelByIndex( int index, bool addTocache )
		{
			LogEntryViewModel logEntryViewModel;

			if ( !viewModelsCache.TryGetValue( index, out logEntryViewModel ) )
			{
				LogEntry logEntry = logEntries[index];
				LogFileViewModel fileViewModel = fileViewModelFactory( logEntry );

				logEntryViewModel = new LogEntryViewModel( logEntry, fileViewModel, this, parent, index );

				if ( addTocache )
				{
					viewModelsCache.Add(index, logEntryViewModel);
					ItemCreated.Raise(this, new LogEntryHostChangedEventArgs(logEntryViewModel));
				}
			}

			return logEntryViewModel;
		}

		object IList.this[int index]
		{
			get
			{
				return this[index];
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		public int IndexOf( object value )
		{
			LogEntryViewModel viewModelEntry = value as LogEntryViewModel;
			if ( viewModelEntry != null )
			{
				return viewModelEntry.IndexInParentCollection;
			}
			else
			{
				LogEntry logEntry = (LogEntry)value;

				int index = logEntries.SequentialIndexOf( logEntry );
				// тут элемент всегда ожидается в коллекции
				if ( index < 0 )
				{
					Condition.BreakIfAttached();
				}

				return index;
			}
		}

		public object SyncRoot
		{
			get { return logEntries; }
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<LogEntryViewModel> GetEnumerator()
		{
			IndexerEnumerator<LogEntryViewModel> enumerator = new IndexerEnumerator<LogEntryViewModel>( this );
			return enumerator;
		}

		public int Count
		{
			get { return logEntries.Count; }
		}

		#region IList<LogEntryViewModel> Members

		public int IndexOf( LogEntryViewModel item )
		{
			throw new NotImplementedException();
		}

		public void Insert( int index, LogEntryViewModel item )
		{
			throw new NotImplementedException();
		}

		public void RemoveAt( int index )
		{
			throw new NotImplementedException();
		}

		#endregion

		#region ICollection<LogEntryViewModel> Members

		public void Add( LogEntryViewModel item )
		{
			throw new NotImplementedException();
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public bool Contains( LogEntryViewModel item )
		{
			throw new NotImplementedException();
		}

		public void CopyTo( LogEntryViewModel[] array, int arrayIndex )
		{
			throw new NotImplementedException();
		}

		public bool IsReadOnly
		{
			get { throw new NotImplementedException(); }
		}

		public bool Remove( LogEntryViewModel item )
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IEnumerable<LogEntryViewModel> Members

		#endregion

		#region IEnumerable Members

		#endregion

		#region IList Members

		public int Add( object value )
		{
			throw new NotImplementedException();
		}

		public bool Contains( object value )
		{
			// todo это правильно?
			return true;
		}

		public void Insert( int index, object value )
		{
			throw new NotImplementedException();
		}

		public bool IsFixedSize
		{
			get { throw new NotImplementedException(); }
		}

		public void Remove( object value )
		{
			throw new NotImplementedException();
		}

		#endregion

		#region ICollection Members

		public void CopyTo( Array array, int index )
		{
			throw new NotImplementedException();
		}

		public bool IsSynchronized
		{
			get { throw new NotImplementedException(); }
		}

		#endregion
	}
}
