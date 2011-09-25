using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace LogAnalyzer.Extensions
{
	public static class BlockingCollectionExtensions
	{
		public static IList<T> WaitForAdded<T>( this BlockingCollection<T> collection, int count )
		{
			List<T> result = new List<T>( count );

			for ( int i = 0; i < count; i++ )
			{
				T item = collection.Take();
				result.Add( item );
			}

			return result;
		}

		public static void WaitForCondition<T>( this BlockingCollection<T> collection, Func<bool> condition )
		{
			while ( !condition() )
			{
				collection.Take();
			}
		}
	}
}
