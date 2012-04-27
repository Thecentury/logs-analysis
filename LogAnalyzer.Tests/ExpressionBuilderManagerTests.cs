using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Filters;
using LogAnalyzer.Kernel;
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

			Assert.That( builders.OfType<GetDateByFileNameBuilder>().Any() );
		}

		[Test]
		public void ShouldNotIncludeFileNameEqualsFilterForIFileInfo()
		{
			var builders = ExpressionBuilderManager.GetBuilders( typeof( bool ), typeof( IFileInfo ) );

			Assert.That( !builders.Any( b => b is FileNameEquals ) );
			Assert.That( !builders.Any( b => b is FileNameNotEquals ) );
		}
	}
}
