using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.Extensions
{
	public static class EnumerableExtensions
	{
		public static bool IsSorted<T>( this IList<T> collection, IComparer<T> comparer )
		{
			for ( int i = 0; i < collection.Count - 1; i++ )
			{
				var current = collection[i];
				var next = collection[i + 1];
				int comparison = comparer.Compare( current, next );
				bool lessThanOrEqual = comparison <= 0;
				if ( !lessThanOrEqual )
				{
					Condition.BreakIfAttached();
					return false;
				}
			}

			return true;
		}

		public static IEnumerable<T> ToIndexerEnumerable<T>( this IList<T> list )
		{
			return new IndexerEnumerable<T>( list );
		}
	}
}
