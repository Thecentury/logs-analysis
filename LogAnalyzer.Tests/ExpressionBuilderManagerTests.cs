using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Filters;
using NUnit.Framework;

namespace LogAnalyzer.Tests
{
	[TestFixture]
	public class ExpressionBuilderManagerTests
	{
		[Test]
		public void ShouldIncludeGetDateByFileNameFilterIfObjectIsRequested()
		{
			var builders = ExpressionBuilderManager.GetBuilders( typeof( object ), typeof( LogEntry ) );

			Assert.That( builders, Has.Some.TypeOf<GetDateByFileNameBuilder>() );
		}
	}
}
