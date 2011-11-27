using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.ColorOverviews;
using NUnit.Framework;

namespace LogAnalyzer.Tests
{
	[TestFixture]
	public class OverviewBuildersTests
	{
		[Test]
		[TestCase( 1, 1 )]
		[TestCase( 10, 10 )]
		[TestCase( 10, 20 )]
		public void TestFirstMatching( int segmentsCount, int entriesCount )
		{
			var builder = new FirstMatchingOverviewCollector<TimeClass>( t => true ) { SegmentsCount = segmentsCount };
			AssertCounts( segmentsCount, entriesCount, builder );
		}

		[Test]
		[TestCase( 1, 1 )]
		[TestCase( 10, 10 )]
		[TestCase( 10, 20 )]
		public void TestLastMatching( int segmentsCount, int entriesCount )
		{
			var builder = new FirstMatchingOverviewCollector<TimeClass>( t => true ) { SegmentsCount = segmentsCount };
			AssertCounts( segmentsCount, entriesCount, builder );
		}

		private static void AssertCounts( int segmentsCount, int entriesCount, OverviewCollectorBase<TimeClass, TimeClass> builder )
		{
			var list = CreateSampleData( entriesCount );
			var overview = builder.Build( list );

			Assert.NotNull( overview );
			Assert.AreEqual( segmentsCount, overview.Length );
		}

		private static List<TimeClass> CreateSampleData( int entriesCount )
		{
			return Enumerable.Range( 0, entriesCount )
				.Select( i => new TimeClass( DateTime.Now.Date.AddDays( i ) ) )
				.ToList();
		}
	}

	internal sealed class TimeClass : IHaveTime
	{
		public TimeClass( DateTime time )
		{
			Time = time;
		}

		public DateTime Time { get; set; }
	}
}
