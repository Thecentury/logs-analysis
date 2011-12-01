using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.GUI.ViewModels.Helpers;
using NUnit.Framework;

namespace LogAnalyzer.Tests.Gui
{
	[TestFixture]
	public class FileSizeHelperTests
	{
		[TestCase( 1000, "1000 Bytes" )]
		[TestCase( 10000, "9.8 Kb" )]
		[TestCase( 1024 * 1024 + 1, "1 Mb" )]
		[TestCase( 2 * 1024L * 1024 * 1024 + 1, "2 Gb" )]
		public void TestBytesFormatting( long length, string expected )
		{
			string actual = FileSizeHelper.GetFormattedLength( length );
			Assert.AreEqual( expected, actual );
		}
	}
}
