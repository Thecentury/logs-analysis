﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using LogAnalyzer.Extensions;
using LogAnalyzer.Filters;
using System.Xaml;
using LogAnalyzer.Kernel;
using Moq;
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
		public void IncludeFilesByNameFilterTest()
		{
			const string fileName1 = "ModuleOrderManager.log";
			const string fileName2 = "Kernal.log";

			var fileMock1 = CreateFileInfoMock( fileName1 );
			var fileMock2 = CreateFileInfoMock( fileName2 );

			IncludeFilesByNameFilter builder = new IncludeFilesByNameFilter();
			builder.FileNames.Add( fileName1 );

			var filter = builder.BuildFilter<IFileInfo>();

			Assert.IsTrue( filter.Include( fileMock1.Object ) );
			Assert.IsFalse( filter.Include( fileMock2.Object ) );
		}

		[Test]
		public void IncludeFilesByNameInsideOtherFiltersTest()
		{
			const string fileName1 = "ModuleOrderManager.log";
			const string fileName2 = "Kernal.log";

			var fileMock2 = CreateFileInfoMock( fileName2 );

			IncludeFilesByNameFilter builder1 = new IncludeFilesByNameFilter();
			builder1.FileNames.Add( fileName1 );

			IncludeFilesByNameFilter builder2 = new IncludeFilesByNameFilter();
			builder1.FileNames.Add( fileName1 );

			AndCollection and = new AndCollection( builder1, builder2 );

			var filter = and.BuildFilter<IFileInfo>();

			Assert.IsFalse( filter.Include( fileMock2.Object ) );
		}

		private static Mock<IFileInfo> CreateFileInfoMock( string fileName1 )
		{
			var fileMock = new Mock<IFileInfo>();
			fileMock.SetupGet( f => f.FullName ).Returns( @"C:\most\awad\logs\" );
			fileMock.SetupGet( f => f.Name ).Returns( fileName1 );
			return fileMock;
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
			ExpressionBuilder[] buildersReturningBoolean = ExpressionBuilderManager.GetBuilders( typeof( bool ), typeof( object ) );
			Assert.NotNull( buildersReturningBoolean );
			Assert.That( buildersReturningBoolean.Length, Is.GreaterThan( 0 ) );

			ExpressionBuilder[] buildersReturningExpression = ExpressionBuilderManager.GetBuilders( typeof( ExpressionBuilder ), typeof( object ) );
			Assert.NotNull( buildersReturningExpression );
			Assert.That( buildersReturningExpression.Length, Is.GreaterThan( 0 ) );
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
						 ExpressionBuilder.CreateConstant( 0 ),
						 ExpressionBuilder.CreateConstant( 1 )
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

		[Test]
		public void BinaryBuilderChildrenValidationTest()
		{
			And and1 = new And();
			Assert.IsFalse( and1.ValidateProperties() );

			And and2 = new And { Left = new Argument() };
			Assert.IsFalse( and2.ValidateProperties() );

			And and3 = new And { Right = new Argument() };
			Assert.IsFalse( and3.ValidateProperties() );
		}

		[Test]
		public void TestAndCollectionValidation()
		{
			AndCollection and1 = new AndCollection();
			Assert.IsFalse( and1.ValidateProperties() );

			AndCollection and2 = new AndCollection();
			and2.Children.Add( null );
			and2.Children.Add( null );
			Assert.IsFalse( and2.ValidateProperties() );

			AndCollection and3 = new AndCollection();
			and3.Children.Add( new AlwaysTrue() );
			and3.Children.Add( new AlwaysTrue() );
			Assert.IsTrue( and3.ValidateProperties() );

			AndCollection and4 = new AndCollection();
			and4.Children.Add( new AlwaysTrue() );
			and4.Children.Add( new AlwaysTrue() );
			and4.Children.Add( null );
			Assert.IsFalse( and4.ValidateProperties() );
		}

		[Test]
		public void TestAndCollection()
		{
			AndCollection and = new AndCollection();
			and.Children.Add( new AlwaysTrue() );
			and.Children.Add( new AlwaysTrue() );
			and.Children.Add( new AlwaysTrue() );
			and.Children.Add( new AlwaysFalse() );

			var compiled = and.BuildFilter<object>();
			bool include = compiled.Include( null );
			Assert.IsFalse( include );
		}

		[Test]
		public void TestAndAlso()
		{
			AndCollection and = new AndCollection();
			and.Children.Add( new GetProperty( new Argument(), "False" ) );
			and.Children.Add( new GetProperty( new Argument(), "Throws" ) );

			var compiled = and.BuildFilter<ThrowingClass>();
			bool include = compiled.Include( new ThrowingClass() );
			Assert.IsFalse( include );
		}

		[Test]
		public void TestOrElse()
		{
			OrCollection or = new OrCollection();
			or.Children.Add( new GetProperty( new Argument(), "True" ) );
			or.Children.Add( new GetProperty( new Argument(), "Throws" ) );

			var compiled = or.BuildFilter<ThrowingClass>();
			bool include = compiled.Include( new ThrowingClass() );
			Assert.IsTrue( include );
		}

		[Test]
		public void ShouldCompile()
		{
			StringConstant str = new StringConstant( "" );
			var func = str.CompileToFunc<string>();
			string value = func();

			Assert.That( value, Is.EqualTo( "" ) );
		}

		[Test]
		public void ShouldFailPatternValidation()
		{
			RegexMatchesFilterBuilder regex = new RegexMatchesFilterBuilder { Substring = new StringConstant( "[a-z" ), Inner = new Argument() };
			Assert.IsFalse( regex.ValidateProperties() );
		}

		[Test]
		public void ShouldSucceedPatternValidation()
		{
			RegexMatchesFilterBuilder regex = new RegexMatchesFilterBuilder { Substring = new StringConstant( "[a-z]+" ), Inner = new Argument() };
			Assert.IsTrue( regex.ValidateProperties() );
		}

		[Test]
		public void RegexFilterShouldWork()
		{
			RegexMatchesFilterBuilder regex = new RegexMatchesFilterBuilder( @"\d{2}", new Argument() );
			var filter = regex.BuildFilter<string>();

			Assert.IsTrue( filter.Include( "22" ) );
			Assert.IsFalse( filter.Include( "2a" ) );
		}

		private sealed class ThrowingClass
		{
			[UsedImplicitly]
			public bool Throws
			{
				get { throw new NotSupportedException(); }
			}

			[UsedImplicitly]
			public bool True
			{
				get { return true; }
			}

			[UsedImplicitly]
			public bool False
			{
				get { return false; }
			}
		}
	}
}
