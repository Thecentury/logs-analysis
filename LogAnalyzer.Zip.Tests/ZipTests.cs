using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zip;
using LogAnalyzer.Config;
using LogAnalyzer.Filters;
using LogAnalyzer.Kernel;
using LogAnalyzer.Kernel.Parsers;
using NUnit.Framework;

namespace LogAnalyzer.Zip.Tests
{
	[TestFixture]
	public class ZipTests
	{
		private const string zipFileName = "Logs.zip";

		// Zip file structure:
		// XslTransform.log
		// Logs1/
		//		PageBuilder-Eticket_Actual.log
		//		XslTransform.log
		//		Inner/
		//				CorporatorManager.log

		[TestCase( "Logs.zip" )]
		[TestCase( "Logs.zip|Logs1/Inner" )]
		[Test]
		public void TestZipFactory( string fileName )
		{
			DirectoryManager manager = new DirectoryManager();
			manager.RegisterCommonFactories();

			manager.RegisterFactory( new ZipDirectoryFactory() );

			LogDirectoryConfigurationInfo directoryConfiguration = new LogDirectoryConfigurationInfo( fileName, "DisplayName" );

			ZipDirectoryInfo zipDirectory = manager.CreateDirectory( directoryConfiguration ) as ZipDirectoryInfo;

			Assert.NotNull( zipDirectory );
		}

		[Test]
		public void TestGetZippedLogs()
		{
			ZipDirectoryInfo zip = new ZipDirectoryInfo( new LogDirectoryConfigurationInfo( zipFileName, "" ), zipFileName, null );
			var files = zip.EnumerateFiles().ToList();

			Assert.AreEqual( 4, files.Count );

			foreach ( IFileInfo fileInfo in files )
			{
				Assert.NotNull( fileInfo );
				var reader = fileInfo.GetReader( new LogFileReaderArguments
				{
					Encoding = Encoding.GetEncoding( 1251 ),
					GlobalEntriesFilter = new DelegateFilter<LogEntry>( logEntry => true ),
					LineParser = new MostLogLineParser()
				} );

				var entries = reader.ReadEntireFile();
				Assert.Greater( entries.Count, 0 );
			}
		}

		[Test]
		public void TestSetRootFolder()
		{
			ZipDirectoryInfo zip = new ZipDirectoryInfo( new LogDirectoryConfigurationInfo( zipFileName, "" ), zipFileName, "Logs1" );
			var files = zip.EnumerateFiles().ToList();

			Assert.AreEqual( 2, files.Count );
		}

		[Test]
		public void TestSetRootFolderWithNestedEnabled()
		{
			ZipDirectoryInfo zip = new ZipDirectoryInfo( new LogDirectoryConfigurationInfo( zipFileName, "" ) { IncludeNestedDirectories = true }, zipFileName, "Logs1" );
			var files = zip.EnumerateFiles().ToList();

			Assert.AreEqual( 3, files.Count );
		}

		[Test]
		public void TestSetDeepRootFolderWithNestedEnabled()
		{
			ZipDirectoryInfo zip = new ZipDirectoryInfo( new LogDirectoryConfigurationInfo( zipFileName, "" ), zipFileName, "Logs1/Inner" );
			var files = zip.EnumerateFiles().ToList();

			Assert.AreEqual( 1, files.Count );
		}
	}
}
