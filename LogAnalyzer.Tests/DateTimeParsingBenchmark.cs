using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using LogAnalyzer.Kernel;
using NUnit.Framework;

namespace LogAnalyzer.Tests
{
	[TestFixture]
	public class DateTimeParsingBenchmark
	{
		public static readonly string DateTimeFormat = "dd.MM.yyyy H:mm:ss";
		private const string sample = "24.05.2011 0:00:47";
		private const int count = 1000000;

		[Test]
		public void StandartParse()
		{
			Stopwatch timer = Stopwatch.StartNew();

			for ( int i = 0; i < count; i++ )
			{
				DateTime date;
				DateTime.TryParseExact( sample, DateTimeFormat, null, DateTimeStyles.None, out date );
			}

			var elapsed = timer.ElapsedMilliseconds;
			Debug.WriteLine( "DateTime.Parse: " + elapsed );
		}

		[Test]
		public void ParseAsInt()
		{
			Stopwatch timer = Stopwatch.StartNew();

			for ( int i = 0; i < count; i++ )
			{
				DateTime date = MostLogLineParser.Parse( sample );
			}

			var elapsed = timer.ElapsedMilliseconds;
			Debug.WriteLine( "Parse as int: " + elapsed );
		}

		[Test]
		public void TestParseAsInt()
		{
			var dt = MostLogLineParser.Parse( sample );
			Assert.AreEqual( new DateTime( 2011, 5, 24, 0, 0, 47 ), dt );

			var dt2 = MostLogLineParser.Parse( "24.05.2011 11:12:13" );
			Assert.AreEqual( new DateTime( 2011, 5, 24, 11, 12, 13 ), dt2 );
		}
	}
}
