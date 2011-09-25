using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace LogAnalyzer.Misc
{
	[DebuggerDisplay("KeysCount = {Count} TotalCount = {TotalCount}")]
	internal class MultiDictionary<TKey, TCollection, TValue> : Dictionary<TKey, TCollection>
		where TCollection : class, ICollection<TValue>, new()
	{
		public MultiDictionary( int capacity ) : base( capacity ) { }

		public void AddAllElements( IEnumerable<IGrouping<TKey, TValue>> collection )
		{
			foreach ( var group in collection )
			{
				var key = group.Key;
				foreach ( var item in group )
				{
					Append( key, item );
				}
			}
		}

		public void Append( TKey key, TValue value )
		{
			TCollection collection = null;
			if ( !base.TryGetValue( key, out collection ) )
			{
				collection = new TCollection();
				base.Add( key, collection );
			}

			collection.Add( value );
		}

		public void Remove( TKey key, TValue value )
		{
			var collection = base[key];
			collection.Remove( value );

			if ( collection.Count == 0 )
			{
				Remove( key );
			}
		}

		/// <summary>
		/// Общее число записей.
		/// </summary>
		public int TotalCount
		{
			get { return Keys.Sum( key => base[key].Count ); }
		}
	}

	internal sealed class AwaitingLogEntriesCollection : MultiDictionary<LogFile, HashSet<LogEntry>, LogEntry>
	{
		public AwaitingLogEntriesCollection( int capacity ) : base( capacity ) { }
	}
}
