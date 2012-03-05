using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace LogAnalyzer.Collections
{
	public sealed class MergingEnumerator : IBidirectionalEnumerator<LogEntry>
	{
		private readonly List<IBidirectionalEnumerator<LogEntry>> _children = new List<IBidirectionalEnumerator<LogEntry>>();
		private readonly SortedSet<LogEntry> _entriesSet = new SortedSet<LogEntry>( new LogEntryByDateComparer() );
		private readonly Dictionary<LogEntry, IBidirectionalEnumerator<LogEntry>> _entryToSourceMap = new Dictionary<LogEntry, IBidirectionalEnumerator<LogEntry>>();

		public MergingEnumerator( [NotNull] IEnumerable<IBidirectionalEnumerable<LogEntry>> enumerables )
		{
			if ( enumerables == null )
			{
				throw new ArgumentNullException( "enumerables" );
			}

			_children.AddRange( enumerables.Select( e => e.GetEnumerator() ) );
		}

		public bool MoveBack()
		{
			// todo brinchuk видимо нужно переключение с хода вперед на ход назад, которое будет очищать буферы
			InitialPopulateBackward();

			if ( _entriesSet.Count == 0 )
			{
				return false;
			}
			else
			{
				LogEntry max = _entriesSet.Max;
				_entriesSet.Remove( max );
				_current = max;
				var enumerator = _entryToSourceMap[max];
				_entryToSourceMap.Remove( max );
				if ( enumerator.MoveBack() )
				{
					_entriesSet.Add( enumerator.Current );
					_entryToSourceMap.Add( enumerator.Current, enumerator );
				}
				return true;
			}
		}

		public void Dispose()
		{
			foreach ( var enumerator in _children )
			{
				enumerator.Dispose();
			}
			_children.Clear();
		}

		public bool MoveNext()
		{
			InitialPopulateForward();

			if ( _entriesSet.Count == 0 )
			{
				return false;
			}
			else
			{
				LogEntry min = _entriesSet.Min;
				_entriesSet.Remove( min );
				_current = min;
				var enumerator = _entryToSourceMap[min];
				_entryToSourceMap.Remove( min );
				if ( enumerator.MoveNext() )
				{
					_entriesSet.Add( enumerator.Current );
					_entryToSourceMap.Add( enumerator.Current, enumerator );
				}
				return true;
			}
		}

		private void InitialPopulateBackward()
		{
			if ( _entriesSet.Count == 0 )
			{
				foreach ( var enumerator in _children )
				{
					if ( enumerator.MoveBack() )
					{
						_entriesSet.Add( enumerator.Current );
						_entryToSourceMap.Add( enumerator.Current, enumerator );
					}
				}
			}
		}

		private void InitialPopulateForward()
		{
			if ( _entriesSet.Count == 0 )
			{
				foreach ( var enumerator in _children )
				{
					if ( enumerator.MoveNext() )
					{
						_entriesSet.Add( enumerator.Current );
						_entryToSourceMap.Add( enumerator.Current, enumerator );
					}
				}
			}
		}

		public void Reset()
		{
			throw new NotSupportedException();
		}

		private LogEntry _current;
		public LogEntry Current
		{
			get { return _current; }
		}

		object IEnumerator.Current
		{
			get { return Current; }
		}
	}
}