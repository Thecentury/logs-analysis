using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using LogAnalyzer.Collections;
using NUnit.Framework;

namespace LogAnalyzer.Tests
{
	[TestFixture]
	public class ParallelIndexOfBenchmark
	{
		private readonly Random rnd = new Random();

		private static List<object> CreateList( int count )
		{
			List<object> result = new List<object>( count );

			for ( int i = 0; i < count; i++ )
			{
				result.Add( new object() );
			}

			return result;
		}

		/// <summary>
		/// Параллельный поиск индекса даже на 10 миллионах работает медленнее.
		/// </summary>
		/// <param name="count"></param>
		[TestCase( 10000000 )]
		[Test]
		public void CompareSortingDurations( int count )
		{
			var list = CreateList( count );

			int index = rnd.Next( list.Count );
			object target = list[index];

			Stopwatch timer = Stopwatch.StartNew();
			int parallelIndex = ParallelHelper.AssuredParallelIndexOf( list, target );
			long parallelDuration = timer.ElapsedMilliseconds;
			Console.WriteLine( "Parallel Duration: " + parallelDuration );

			timer = Stopwatch.StartNew();
			int sequentialIndex = ParallelHelper.SequentialIndexOf( list, target );
			long sequentialDuration = timer.ElapsedMilliseconds;
			Console.WriteLine( "Sequential Duration: " + sequentialDuration );
		}
	}
}
