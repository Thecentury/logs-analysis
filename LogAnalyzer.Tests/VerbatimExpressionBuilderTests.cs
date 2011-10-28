using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Filters;
using NUnit.Framework;

namespace LogAnalyzer.Tests
{
	[TestFixture]
	public class VerbatimExpressionBuilderTests
	{
		[Test]
		public void TestVerbatimExpressionBuilder()
		{
			var builder = ExpressionBuilder.Create<int>( i => i % 2 == 0 );
			var compiled = builder.BuildFilter<int>();

			Assert.IsTrue( compiled.Include( 0 ) );
			Assert.IsFalse( compiled.Include( 1 ) );
		}
	}
}
