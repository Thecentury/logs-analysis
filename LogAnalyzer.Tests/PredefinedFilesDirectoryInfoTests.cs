using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Config;
using LogAnalyzer.Kernel;
using NUnit.Framework;

namespace LogAnalyzer.Tests
{
	[TestFixture]
	public class PredefinedFilesDirectoryInfoTests
	{
		[Test]
		public void ShouldReturnAListOfFiles()
		{
			var cfg = new LogDirectoryConfigurationInfo();
			cfg.PredefinedFiles.Add( "1.log" );
			PredefinedFilesDirectoryInfo directory = new PredefinedFilesDirectoryInfo( cfg );

			var files = directory.EnumerateFileNames().ToList();
			Assert.That( files, Has.Count.EqualTo( 1 ) );
		}
	}
}
