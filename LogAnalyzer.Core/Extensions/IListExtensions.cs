﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Collections;

namespace LogAnalyzer.Extensions
{
	public static class IListExtensions
	{
		public static void RemoveLastItem<T>( this IList<T> list )
		{
			int count = list.Count;
			if ( count > 0 )
			{
				list.RemoveAt( count - 1 );
			}
		}

		public static T Second<T>( this IList<T> list )
		{
			return list[1];
		}

		public static T Second<T>( this IEnumerable<T> collection )
		{
			return collection.Skip( 1 ).First();
		}

		/// <summary>
		/// Поиск индекса, уверенный в том, что коллекция содержит искомый элемент.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="collection"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		[Obsolete( "Use SequentialIndexOf", true )]
		public static int ParallelAssuredIndexOf<T>( this IList<T> collection, T item ) where T : class
		{
			return ParallelHelper.AssuredParallelIndexOf( collection, item );
		}

		public static int SequentialIndexOf<T>( this IList<T> collection, T item ) where T : class
		{
			return ParallelHelper.SequentialIndexOf( collection, item );
		}
	}
}
