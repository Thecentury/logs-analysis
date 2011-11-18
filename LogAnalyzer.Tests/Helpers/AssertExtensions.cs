using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using LogAnalyzer.Extensions;
using NUnit.Framework;

namespace LogAnalyzer.Tests
{
	internal static class AssertExtensions
	{
		/// <summary>
		/// Проверяет, отсортирована ли последовательность.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="collection"></param>
		/// <param name="comparer"></param>
		[DebuggerStepThrough]
		public static void AssertIsSorted<T>( this IList<T> collection, IComparer<T> comparer )
		{
			Assert.IsTrue( collection.IsSorted( comparer ) );
		}

		[DebuggerStepThrough]
		public static void AssertIsTrueOrFailWithMessage( this bool value, string message = "False" )
		{
			Assert.IsTrue( value, message );
		}
	}
}
