using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Extensions;
using LogAnalyzer.Filters;
using System.Xaml;
using NUnit.Framework;

namespace LogAnalyzer.Tests
{
	[TestFixture]
	public class FilterTests
	{
		[Test]
		public void TestStringContainsFilter()
		{
			StringContains builder = new StringContains { Substring = ExpressionBuilder.CreateConstant( "1" ), Inner = new Argument() };
			var filter = builder.BuildFilter<string>();

			Assert.IsTrue( filter.Include( "123" ) );
			Assert.IsFalse( filter.Include( "456" ) );
		}


		[Test]
		public void TestStringStartsWithFilter()
		{
			StringStartsWith builder = new StringStartsWith
			{
				Comparison = StringComparison.InvariantCultureIgnoreCase,
				Substring = ExpressionBuilder.CreateConstant( "A" ),
				Inner = new Argument()
			};

			var filter = builder.BuildFilter<string>();

			Assert.IsTrue( filter.Include( "aBC" ) );
			Assert.IsFalse( filter.Include( "DAe" ) );
		}

		[Test]
		public void TestPropertyAccessFilter()
		{
			GetProperty builder = new GetProperty { PropertyName = "Length", Target = new Argument() };
			Type resultType = builder.GetResultType<string>();

			Assert.AreEqual( typeof( int ), resultType );
		}

		[Test]
		public void TestExpressionBuilderManager()
		{
			ExpressionBuilder[] buildersReturningBoolean = ExpressionBuilderManager.GetBuildersReturningType( typeof( bool ) );
			ExpressionBuilder[] buildersReturningExpression = ExpressionBuilderManager.GetBuildersReturningType( typeof( ExpressionBuilder ) );
		}

		[Test]
		public void TestGreaterThanBuilder()
		{
			GreaterThan greaterBuilder = CreateSampleGreaterThanBuilder();

			var filter = greaterBuilder.BuildFilter<object>();
			Assert.IsFalse( filter.Include( null ) );
		}

		private static GreaterThan CreateSampleGreaterThanBuilder()
		{
			GreaterThan greaterBuilder = BinaryExpressionBuilder.Create<GreaterThan>(
						 ExpressionBuilder.CreateConstant<int>( 0 ),
						 ExpressionBuilder.CreateConstant<int>( 1 )
						 );
			return greaterBuilder;
		}

		[Test]
		public void TestExpressionBuilderXamlSerialization()
		{
			GreaterThan greaterBuilder = CreateSampleGreaterThanBuilder();

			string xaml = XamlServices.Save( greaterBuilder );

			Assert.IsTrue( !xaml.IsNullOrEmpty() );
		}

		[Test]
		public void TestEqualsDeserialization()
		{
			const string xaml =
				@"<Equals xmlns=""http://www.awad.com/LogAnalyzer"">
					<GetProperty PropertyName=""ThreadId""/>
					<IntConstant Value=""62""/>
				</Equals>";

			ExpressionBuilder builder = (ExpressionBuilder)XamlServices.Parse( xaml );
			Assert.NotNull( builder );
		}

		[Test]
		public void TestBinaryOperatorDeserialization()
		{
			const string xaml =
				@"<And xmlns=""http://www.awad.com/LogAnalyzer"">
					<AlwaysFalse/>
					<AlwaysTrue/>
				</And>";

			ExpressionBuilder builder = (ExpressionBuilder)XamlServices.Parse( xaml );
			Assert.NotNull( builder );

			var compiled = builder.BuildFilter<int>();
			bool include = compiled.Include( 2 );
			Assert.IsFalse( include );
		}
	}
}
