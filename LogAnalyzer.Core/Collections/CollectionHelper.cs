using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.Collections
{
	internal static class CollectionHelper
	{
		public static IList<T> CreateList<T>()
		{
#if DEBUG
			return new SingleThreadWriteCollection<T>();
#else
			return new List<T>();
#endif
		}
	}
}
