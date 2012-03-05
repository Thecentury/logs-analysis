using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace LogAnalyzer.Collections
{
	public sealed class MergingNavigator : IBidirectionalEnumerable<LogEntry>
	{
		private readonly List<IBidirectionalEnumerable<LogEntry>> _children = new List<IBidirectionalEnumerable<LogEntry>>();

		public MergingNavigator( [NotNull] params IBidirectionalEnumerable<LogEntry>[] children )
		{
			if ( children == null )
			{
				throw new ArgumentNullException( "children" );
			}
			_children.AddRange( children );
		}

		public MergingNavigator( [NotNull] IEnumerable<IBidirectionalEnumerable<LogEntry>> children )
		{
			if ( children == null )
			{
				throw new ArgumentNullException( "children" );
			}
			_children.AddRange( children );
		}

		public IBidirectionalEnumerator<LogEntry> GetEnumerator()
		{
			return new MergingEnumerator( _children );
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
}