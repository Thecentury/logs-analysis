using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zip;
using LogAnalyzer.Config;
using LogAnalyzer.Kernel;
using NUnit.Framework;

namespace LogAnalyzer.Zip.Tests
{
	[TestFixture]
	public class ZipTests
	{
		private const string zipFileName = "Logs.zip";

		[Test]
		public void Test1()
		{
			using ( ZipFile zip = new ZipFile( zipFileName ) )
			{
				foreach (ZipEntry zipEntry in zip)
				{
				}
			}
		}

		[TestCase( "Logs.zip" )]
		[TestCase( "Logs.zip|Logs1/Inner" )]
		[Test]
		public void TestZipFactory( string fileName )
		{
			DirectoryManager manager = new DirectoryManager();
			manager.RegisterCommonFactories();

			manager.RegisterFactory( new ZipDirectoryFactory() );

			LogDirectoryConfigurationInfo directoryConfiguration = new LogDirectoryConfigurationInfo( fileName, "*", "DisplayName" );

			ZipDirectoryInfo zipDirectory = manager.CreateDirectory( directoryConfiguration ) as ZipDirectoryInfo;

			Assert.NotNull( zipDirectory );
		}
	}
}
