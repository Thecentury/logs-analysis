using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Helper = LogAnalyzer.Zip.ZipDirectoryInfo.DirectoriesHierarchyHelper;

namespace LogAnalyzer.Zip.Tests
{
	[TestFixture]
	public class DirectoryHierarchyTests
	{
		[Test]
		public void NullRootIncludeNested()
		{
			var helper = new Helper( null, true );

			Assert.IsTrue( helper.IncludeDirectory( "" ) );
			Assert.IsTrue( helper.IncludeDirectory( "A" ) );
			Assert.IsTrue( helper.IncludeDirectory( "A/A" ) );
		}

		[Test]
		public void EmptyRootIncludeNested()
		{
			var helper = new Helper( "", true );

			Assert.IsTrue( helper.IncludeDirectory( "" ) );
			Assert.IsTrue( helper.IncludeDirectory( "A" ) );
			Assert.IsTrue( helper.IncludeDirectory( "A/A" ) );
		}

		[Test]
		public void EmptyRootExcludeNested()
		{
			var helper = new Helper( "", false );

			Assert.IsTrue( helper.IncludeDirectory( "" ) );
			Assert.IsFalse( helper.IncludeDirectory( "A" ) );
			Assert.IsFalse( helper.IncludeDirectory( "A/A" ) );
		}

		[Test]
		public void RootIncludeNested()
		{
			var helper = new Helper( "A", true );

			Assert.IsFalse( helper.IncludeDirectory( "" ) );
			Assert.IsFalse( helper.IncludeDirectory( "B" ) );

			Assert.IsTrue( helper.IncludeDirectory( "A" ) );
			Assert.IsTrue( helper.IncludeDirectory( "A/A" ) );
		}

		[Test]
		public void RootExcludeNested()
		{
			var helper = new Helper( "A", false );

			Assert.IsFalse( helper.IncludeDirectory( "" ) );
			Assert.IsFalse( helper.IncludeDirectory( "B" ) );

			Assert.IsTrue( helper.IncludeDirectory( "A" ) );
			Assert.IsFalse( helper.IncludeDirectory( "A/A" ) );
		}

		[Test]
		public void DeepRootIncludeNested()
		{
			var helper = new Helper( "A/A", true );

			Assert.IsFalse( helper.IncludeDirectory( "" ) );
			Assert.IsFalse( helper.IncludeDirectory( "B" ) );
			Assert.IsFalse( helper.IncludeDirectory( "A" ) );

			Assert.IsTrue( helper.IncludeDirectory( "A/A" ) );
			Assert.IsTrue( helper.IncludeDirectory( "A/A/A" ) );
			Assert.IsTrue( helper.IncludeDirectory( "A/A/B" ) );

			Assert.IsFalse( helper.IncludeDirectory( "A/B" ) );
			Assert.IsFalse( helper.IncludeDirectory( "B/A" ) );
		}
	}
}
