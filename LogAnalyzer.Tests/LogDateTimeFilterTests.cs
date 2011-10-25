using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Filters;
using NUnit.Framework;

namespace LogAnalyzer.Tests
{
	[TestFixture]
	public class LogDateTimeFilterTests
	{
		private sealed class TimeClass
		{
			public DateTime Time { get; set; }
		}

		[Test]
		public void TestGreaterThanFilter()
		{
			LogDateTimeGreater filter = new LogDateTimeGreater( DateTime.Now.Date );
			var compiled = filter.BuildFilter<TimeClass>();

			TimeClass sampleLess = new TimeClass { Time = DateTime.MinValue };
			Assert.IsFalse( compiled.Include( sampleLess ) );

			TimeClass sampleGreater = new TimeClass { Time = DateTime.MaxValue };
			Assert.IsTrue( compiled.Include( sampleGreater ) );
		}

		[Test]
		public void TestLessThanFilter()
		{
			LogDateTimeLess filter = new LogDateTimeLess( DateTime.Now.Date );
			var compiled = filter.BuildFilter<TimeClass>();

			TimeClass sampleLess = new TimeClass { Time = DateTime.MinValue };
			Assert.IsTrue( compiled.Include( sampleLess ) );

			TimeClass sampleGreater = new TimeClass { Time = DateTime.MaxValue };
			Assert.IsFalse( compiled.Include( sampleGreater ) );
		}
	}
}
