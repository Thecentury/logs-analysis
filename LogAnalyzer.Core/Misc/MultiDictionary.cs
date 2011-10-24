using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace LogAnalyzer.Misc
{
	[DebuggerDisplay( "KeysCount = {Count} TotalCount = {TotalCount}" )]
	public class MultiDictionary<TKey, TCollection, TValue> : Dictionary<TKey, TCollection>
		where TCollection : class, ICollection<TValue>
	{
		private readonly Func<TCollection> createCollectionFunc;

		public MultiDictionary( Func<TCollection> createCollectionFunc )
		{
			if ( createCollectionFunc == null ) throw new ArgumentNullException( "createCollectionFunc" );
			this.createCollectionFunc = createCollectionFunc;
		}

		public MultiDictionary( Func<TCollection> createCollectionFunc, IEqualityComparer<TKey> comparer )
			: base( comparer )
		{
			if ( createCollectionFunc == null ) throw new ArgumentNullException( "createCollectionFunc" );
			this.createCollectionFunc = createCollectionFunc;
		}

		public MultiDictionary( Func<TCollection> createCollectionFunc, int capacity )
			: base( capacity )
		{
			if ( createCollectionFunc == null ) throw new ArgumentNullException( "createCollectionFunc" );
			this.createCollectionFunc = createCollectionFunc;
		}

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
			TCollection collection;
			if ( !TryGetValue( key, out collection ) )
			{
				collection = createCollectionFunc();
				Add( key, collection );
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
		public AwaitingLogEntriesCollection( int capacity ) : base( () => new HashSet<LogEntry>(), capacity ) { }
	}

	public sealed class ListMultiDictionary<TKey, TValue> : MultiDictionary<TKey, List<TValue>, TValue>
	{
		public ListMultiDictionary() : base( () => new List<TValue>() ) { }

		public ListMultiDictionary( IEqualityComparer<TKey> comparer ) : base( () => new List<TValue>(), comparer ) { }
	}
}
