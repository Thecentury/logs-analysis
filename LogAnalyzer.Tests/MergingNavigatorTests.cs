using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Collections;
using NUnit.Framework;

namespace LogAnalyzer.Tests
{
	[TestFixture]
	public class MergingNavigatorTests
	{
		[Test]
		public void ShouldProcessEmptyListOfEnumerators()
		{
			MergingNavigator navigator =
				new MergingNavigator( BidirectionalEnumerable.CreateForwardOnly( Enumerable.Empty<LogEntry>() ) );
			var enumerator = navigator.GetEnumerator();

			Assert.IsFalse( enumerator.MoveNext() );
			Assert.IsFalse( enumerator.MoveBack() );
		}

		[Test]
		public void ShouldProcessListOfEnumeratorsOfOneItem()
		{
		}

		[Test]
		public void ShouldMerge()
		{
			List<LogEntry> entries1 = new List<LogEntry>
			                          	{
			                          		new LogEntry( "I", 0, new DateTime( 2012, 01, 01 ), "line1", 0, null ),
			                          		new LogEntry( "I", 0, new DateTime( 2012, 01, 03 ), "line2", 0, null )
			                          	};
			var bi1 = BidirectionalEnumerable.CreateForwardOnly( entries1 );

			List<LogEntry> entries2 = new List<LogEntry>
			                          	{
			                          		new LogEntry( "I", 0, new DateTime( 2012, 01, 02 ), "line1", 0, null ),
			                          		new LogEntry( "I", 0, new DateTime( 2012, 01, 04 ), "line2", 0, null )
			                          	};
			var bi2 = BidirectionalEnumerable.CreateForwardOnly( entries2 );

			MergingNavigator navigator = new MergingNavigator( bi1, bi2 );
			var merged = navigator.ToForwardEnumerable().ToList();
			Assert.That( merged.Count, Is.EqualTo( entries1.Count + entries2.Count ) );
		}
	}
}
