#if DEBUG
#define DEBUGCHECK
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using LogAnalyzer.Extensions;
using LogAnalyzer.Kernel;
using LogAnalyzer.Misc;
using System.Collections;

namespace LogAnalyzer
{
	/// <summary>
	/// Осуществляет упорядочивание LogEntry.
	/// </summary>
	[DebuggerDisplay( "Count = {sortedLogEntries.Count}" )]
	internal sealed class LogEntrySortedCollection
	{
		private readonly LogEntriesList parent;
		private readonly Thread allowedAccessThread;
		private readonly ITimeService timeService;

		private readonly CollectionToListWrapper<LogEntry> unappendedSetWrapper;

		public IList<LogEntry> UnappendedLogEntries
		{
			get { return unappendedSetWrapper; }
		}

		private readonly SortedSet<LogEntry> sortedLogEntries = new SortedSet<LogEntry>( LogEntryByDateComparer.Instance );

		public LogEntrySortedCollection( LogEntriesList parent, IEnvironment environment )
		{
			if ( parent == null )
				throw new ArgumentNullException( "parent" );
			if ( environment == null )
				throw new ArgumentNullException( "environment" );

			this.parent = parent;
			allowedAccessThread = environment.OperationsQueue.Thread;
			timeService = environment.TimeService;

			unappendedSetWrapper = new CollectionToListWrapper<LogEntry>( sortedLogEntries );
		}

		/// <summary>
		/// <remarks>
		/// Рассчитывает, что записи будут почти отсортированы по времени, иначе, видимо, будет не производительна.
		/// </remarks>
		/// </summary>
		/// <param name="addedEntries"></param>
		public void Enqueue( IList<LogEntry> addedEntries )
		{
			if ( addedEntries == null )
				throw new ArgumentNullException( "addedEntries" );

			ValidateThread();

#if DEBUGCHECK
			int mergedCount = parent.MergedEntries.Count;

			bool sortedEntriesDoNotContainTheEntrybeingAdded = addedEntries.All( newEntry => !sortedLogEntries.Contains( newEntry ) );
			Condition.DebugAssert( sortedEntriesDoNotContainTheEntrybeingAdded,
				"Коллекция sortedLogEntries не должна содержать добавляемые элементы." );

			bool parentMergedEntriesDoNotContainTheEntryBeingAdded = addedEntries.All( newEntry => !parent.MergedEntries.Select( _ => _ ).Contains( newEntry ) );
			Condition.DebugAssert( parentMergedEntriesDoNotContainTheEntryBeingAdded,
				"Коллекция parent.MergedEntries не должна содержать добавляемые элементы." );

			AssertParentMergedEntriesAreSorted();
#endif

			foreach ( var addedEntry in addedEntries )
			{
				sortedLogEntries.Add( addedEntry );
			}

			Condition.DebugAssert( !sortedLogEntries.Overlaps( parent.MergedEntries.First ),
				"У коллекций sortedLogEntries и parent.MergedEntries не должно быть общих элементов." );

			// todo добавлять в список mergedEntries и по разнице времени между записью и now

			int capacity = sortedLogEntries.Count + addedEntries.Count;
			AwaitingLogEntriesCollection awaitingEntries = new AwaitingLogEntriesCollection( capacity );

			awaitingEntries.AddAllElements( sortedLogEntries.Union( addedEntries ).GroupBy( logEntry => logEntry.ParentLogFile ) );

			// новая запись в файл, из которого тут раньше не было записей

			// новопришедшая запись не может быть раньше уже существующей,
			// так что ничего не делаем.

			// вместе с новоприбывшей записью образуется полный набор LogEntry?
			// если это так, то это значит, что не может быть еще какой-то записи в будущем, 
			// которая будет раньше самой ранней из sortedLogEntries
			// (случай, что есть недобавленный файл, мы пока не рассматриваем)

			int totalFilesCount = parent.TotalFilesCount;

			if ( totalFilesCount > 0 )
			{
				while ( awaitingEntries.Keys.Count >= totalFilesCount )
				{
					LogEntry min = sortedLogEntries.Min;
					parent.AppendLogEntryToMergedList( min );

					int sortedSetLength = sortedLogEntries.Count;
					sortedLogEntries.Remove( min );
					Condition.DebugAssert( sortedLogEntries.Count == (sortedSetLength - 1), "" );

					awaitingEntries.Remove( min.ParentLogFile, min );
				}
			}

			// переносит в parent.MergedEntries записи относительно старые записи лога,
			// т.к. мы считаем, что такие старые записи больше не появятся.
			DateTime now = DateTime.Now;
			bool hasRemoved = false;
			LogEntry max = sortedLogEntries.Max;
			if ( max != null )
			{
				DateTime maxTime = max.Time;

				do
				{
					LogEntry min = sortedLogEntries.Min;
					if ( min == null )
						break;

					if ( timeService.IsRelativelyOld( min.Time, maxTime ) )
					{
#if DEBUGCHECK
						LogEntry last = parent.MergedEntries.Last();
						int comparison = LogEntryByDateComparer.Instance.Compare( last, min );
						Condition.DebugAssert( comparison <= 0, "Добавляемый элемент меньше последнего в списке parent.MergedEntries." );
#endif

						parent.AppendLogEntryToMergedList( min );
						sortedLogEntries.Remove( min );
						hasRemoved = true;
					}
				} while ( hasRemoved );
			}

			unappendedSetWrapper.MarkDirty();

#if DEBUGCHECK
			Condition.DebugAssert( parent.MergedEntries.Count > mergedCount, "Число смерженных записей должно было увеличиться." );
			AssertParentMergedEntriesAreSorted();
#endif

			Logger.Instance.DebugWriteInfo( "LogEntrySortedCollection<" + ParentName + ">.Enqueue: MergedCount = " + parent.MergedEntries.Count );

			parent.RaiseLogEntryAdded( addedEntries );
		}

		[DebuggerStepThrough]
		[Conditional( "DEBUG" )]
		private void AssertParentMergedEntriesAreSorted()
		{
			Condition.DebugAssert( parent.MergedEntries.AreSorted( LogEntryByDateComparer.Instance ),
				"Коллекция parent.MergedEntries должна быть отсортирована." );
		}

		private string ParentName
		{
			get { return parent.GetType().Name; }
		}

		[Conditional( "DEBUG" )]
		private void ValidateThread()
		{
			if ( Thread.CurrentThread != allowedAccessThread )
			{
				Debugger.Launch();
				throw new InvalidOperationException( "Attempt to modify collection from other thread." );
			}
		}

		[DebuggerDisplay( "CollectionToListWrapper<{TypeForDebugger}> Count = {Count}" )]
		private sealed class CollectionToListWrapper<T> : IList<T>, ICollection
		{
			[DebuggerBrowsable( DebuggerBrowsableState.Never )]
			private string TypeForDebugger
			{
				get { return typeof( T ).Name; }
			}

			private readonly ICollection<T> collection = null;
			private T[] array = null;
			private bool isOld = true;
			private readonly object syncRoot = new object();

			public CollectionToListWrapper( ICollection<T> collection )
			{
				if ( collection == null )
					throw new ArgumentNullException( "collection" );

				this.collection = collection;
			}

			public void MarkDirty()
			{
				lock ( syncRoot )
				{
					this.isOld = true;
				}
			}

			int IList<T>.IndexOf( T item )
			{
				throw new NotImplementedException();
			}

			void IList<T>.Insert( int index, T item )
			{
				throw new NotImplementedException();
			}

			void IList<T>.RemoveAt( int index )
			{
				throw new NotImplementedException();
			}

			public T this[int index]
			{
				get
				{
					throw new NotImplementedException();
				}
				set
				{
					throw new NotImplementedException();
				}
			}

			void ICollection<T>.Add( T item )
			{
				throw new NotImplementedException();
			}

			void ICollection<T>.Clear()
			{
				throw new NotImplementedException();
			}

			bool ICollection<T>.Contains( T item )
			{
				throw new NotImplementedException();
			}

			public void CopyTo( T[] array, int arrayIndex )
			{
				lock ( syncRoot )
				{
					UpdateCopy();

					this.array.CopyTo( array, arrayIndex );
				}
			}

			private void UpdateCopy()
			{
				if ( isOld )
				{
					this.array = new T[collection.Count];
					collection.CopyTo( this.array, 0 );
					isOld = false;
				}
			}

			public int Count
			{
				get
				{
					lock ( syncRoot )
					{
						UpdateCopy();

						return array.Length;
					}
				}
			}

			bool ICollection<T>.IsReadOnly
			{
				get { return true; }
			}

			bool ICollection<T>.Remove( T item )
			{
				throw new NotImplementedException();
			}

			IEnumerator<T> IEnumerable<T>.GetEnumerator()
			{
				throw new NotImplementedException();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				throw new NotImplementedException();
			}

			void ICollection.CopyTo( Array array, int index )
			{
				CopyTo( (T[])array, index );
			}

			public bool IsSynchronized
			{
				get { return true; }
			}

			public object SyncRoot
			{
				get { return syncRoot; }
			}
		}
	}
}
