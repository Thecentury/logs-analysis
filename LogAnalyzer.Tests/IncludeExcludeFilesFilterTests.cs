using LogAnalyzer.Filters;
using LogAnalyzer.Kernel;
using Moq;
using NUnit.Framework;

namespace LogAnalyzer.Tests
{
	[TestFixture]
	public class IncludeExcludeFilesFilterTests
	{
		[TestCase( "file", "file" )]
		[TestCase( "2012-04-12-file", "file" )]
		public void IncludeByCleanedNameFilterShouldPass( string fileName, string includedName )
		{
			var builder = new IncludeFilesByCleanedNameFilter( includedName );

			bool includes = FilterIncludes( fileName, builder );

			Assert.IsTrue( includes );
		}

		[TestCase( "file", "file2" )]
		[TestCase( "2012-04-12-file", "file2" )]
		public void IncludeByCleanedNameFilterShouldNotPass( string fileName, string includedName )
		{
			var builder = new IncludeFilesByCleanedNameFilter( includedName );

			bool includes = FilterIncludes( fileName, builder );

			Assert.IsFalse( includes );
		}

		[TestCase( "file", "file2" )]
		[TestCase( "2012-04-12-file", "file2" )]
		public void ExcludeByCleanedNameFilterShouldPass( string fileName, string excludedName )
		{
			var builder = new ExcludeFilesByCleanedNameFilter( excludedName );

			bool includes = FilterIncludes( fileName, builder );

			Assert.IsTrue( includes );
		}

		[TestCase( "file", "file" )]
		[TestCase( "2012-04-12-file", "file" )]
		public void ExcludeByCleanedNameFilterShouldNotPass( string fileName, string excludedName )
		{
			var builder = new ExcludeFilesByCleanedNameFilter( excludedName );

			bool includes = FilterIncludes( fileName, builder );

			Assert.IsFalse( includes );
		}

		private static bool FilterIncludes( string fileName, ExpressionBuilder builder )
		{
			Mock<IFileInfo> fileMock = new Mock<IFileInfo>();
			fileMock.SetupGet( f => f.Name ).Returns( fileName );

			var filter = builder.BuildFilter<IFileInfo>();
			bool includes = filter.Include( fileMock.Object );
			return includes;
		}
	}
}