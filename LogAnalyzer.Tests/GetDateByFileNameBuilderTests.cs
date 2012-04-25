using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using LogAnalyzer.Filters;
using LogAnalyzer.Kernel;
using NUnit.Framework;

namespace LogAnalyzer.Tests
{
	[TestFixture]
	public class GetDateByFileNameBuilderTests
	{
		[TestCase( "2012-10-30-Cache.log", "2012.10.30" )]
		public void ShouldReturnDateFromFileName( string fileName, string expectedDate )
		{
			GetDateByFileNameBuilder builder = new GetDateByFileNameBuilder();
			var func = builder.CompileToFunc<string, DateTime>();

			DateTime actualDate = func( fileName );
			Assert.AreEqual( expectedDate, actualDate.ToString( "yyyy.MM.dd" ) );
		}

		[Test]
		public void ShouldReturnTodayWhenFileNameIsClean()
		{
			GetDateByFileNameBuilder builder = new GetDateByFileNameBuilder();
			var func = builder.CompileToFunc<string, DateTime>();

			DateTime actualDate = func( "Kernel.log" );
			Assert.AreEqual( DateTime.Now.Date, actualDate );
		}

		[Test]
		public void DateByFileNameEqualsShouldBeTrue()
		{
			var builder = new Equals( new GetDateByFileNameBuilder(), new DateTimeConstant( new DateTime( 2012, 10, 30 ) ) );

			var filter = builder.BuildFilter<string>();

			bool include = filter.Include( "2012-10-30-Kernel.log" );

			Assert.IsTrue( include );
		}

		[Test]
		public void DateByFileNameEqualsShouldBeFalse()
		{
			var builder = new Equals( new GetDateByFileNameBuilder(), new DateTimeConstant( new DateTime( 2012, 01, 01 ) ) );

			var filter = builder.BuildFilter<string>();

			bool include = filter.Include( "2012-10-30-Kernel.log" );

			Assert.IsFalse( include );
		}

		[Test]
		public void DateByFileNameEqualsShouldWorkForIFileInfo()
		{
			var builder = new GetDateByFileNameBuilder();

			var expression = builder.CreateExpression( Expression.Parameter( typeof( IFileInfo ) ) );

			Assert.NotNull( expression );
		}
	}
}
