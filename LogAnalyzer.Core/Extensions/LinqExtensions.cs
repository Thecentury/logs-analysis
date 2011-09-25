using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Linq
{
	public static class LinqExtensions
	{
		public static IOrderedEnumerable<T> OrderBy<T>( this IEnumerable<T> collection, IComparer<T> comparer )
		{
			return collection.OrderBy( _ => _, comparer );
		}

		public static OrderedParallelQuery<T> OrderBy<T>( this ParallelQuery<T> source, IComparer<T> comparer )
		{
			return source.OrderBy( _ => _, comparer );
		}
	}
}
