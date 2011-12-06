using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.Extensions
{
	public static class ObjectExtensions
	{
		public static bool In<T>( this T item, T value1, T value2 )
		{
			return item.Equals( value1 ) || item.Equals( value2 );
		}

		public static bool In<T>( this T item, T value1, T value2, T value3 )
		{
			return item.Equals( value1 ) ||
				   item.Equals( value2 ) ||
				   item.Equals( value3 );
		}
	}
}
