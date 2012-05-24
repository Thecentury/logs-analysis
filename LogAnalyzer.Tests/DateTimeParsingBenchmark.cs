using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using LogAnalyzer.Kernel;
using LogAnalyzer.Kernel.Parsers;
using NUnit.Framework;

namespace LogAnalyzer.Tests
{
	[TestFixture]
	public class DateTimeParsingBenchmark
	{
		public static readonly string DateTimeFormat = "dd.MM.yyyy H:mm:ss";
		private const string Sample = "24.05.2011 0:00:47";
		private const int Count = 1000000;

		[Test]
		public void StandartParse()
		{
			Stopwatch timer = Stopwatch.StartNew();

			for ( int i = 0; i < Count; i++ )
			{
				DateTime date;
				DateTime.TryParseExact( Sample, DateTimeFormat, null, DateTimeStyles.None, out date );
			}

			var elapsed = timer.ElapsedMilliseconds;
			Debug.WriteLine( "DateTime.Parse: " + elapsed );
		}

		[Test]
		public void ParseAsInt()
		{
			Stopwatch timer = Stopwatch.StartNew();

			DateTime date;
			for ( int i = 0; i < Count; i++ )
			{
				MostLogLineParser.TryParseDate( Sample, out date );
			}

			var elapsed = timer.ElapsedMilliseconds;
			Debug.WriteLine( "Parse as int: " + elapsed );
		}

		[Test]
		public void TestParseAsInt()
		{
			DateTime date;
			MostLogLineParser.TryParseDate( Sample, out date );
			Assert.AreEqual( new DateTime( 2011, 5, 24, 0, 0, 47 ), date );

			DateTime date2;
			MostLogLineParser.TryParseDate( "24.05.2011 11:12:13", out date2 );
			Assert.AreEqual( new DateTime( 2011, 5, 24, 11, 12, 13 ), date2 );
		}
	}
}
