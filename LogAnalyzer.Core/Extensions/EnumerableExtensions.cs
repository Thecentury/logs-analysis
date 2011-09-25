using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.Extensions
{
	public static class EnumerableExtensions
	{
		public static bool AreSorted<T>( this IEnumerable<T> collection, IComparer<T> comparer )
		{
			return collection.Zip( collection.Skip( 1 ), ( t1, t2 ) => new { T1 = t1, T2 = t2 } )
				.All( t =>
					{
						bool comparison = comparer.Compare( t.T1, t.T2 ) < 0;
						return comparison;
					} );
		}

		public static IEnumerable<T> ToIndexerEnumerable<T>( this IList<T> list )
		{
			return new IndexerEnumerable<T>( list );
		}
	}
}
