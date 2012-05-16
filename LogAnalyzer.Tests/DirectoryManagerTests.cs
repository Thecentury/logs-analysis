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
	public class DirectoryManagerTests
	{
		private readonly DirectoryManager directoryManager = new DirectoryManager();

		[TestFixtureSetUp]
		public void SetUp()
		{
			directoryManager.RegisterFactory( new DefaultDirectoryFactory() );
			directoryManager.RegisterFactory( new PredefinedFilesDirectoryFactory() );
		}

		[Test]
		public void TestHasPredefinedFiles()
		{
			LogDirectoryConfigurationInfo directoryConfiguration = new LogDirectoryConfigurationInfo();
			directoryConfiguration.PredefinedFiles.Add( "file" );

			PredefinedFilesDirectoryInfo dir =
				directoryManager.CreateDirectory( directoryConfiguration ) as PredefinedFilesDirectoryInfo;

			Assert.NotNull( dir );
		}

		[Test]
		public void TestHasNoPredefinedFiles()
		{
			LogDirectoryConfigurationInfo directoryConfiguration = new LogDirectoryConfigurationInfo( "C:/Windows", "Dir" );

			var dir = directoryManager.CreateDirectory( directoryConfiguration );

			Assert.NotNull( dir );
			Assert.IsNotInstanceOf<PredefinedFilesDirectoryInfo>( dir );
		}
	}
}
