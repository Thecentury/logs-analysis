using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zip;
using LogAnalyzer.Config;
using LogAnalyzer.Filters;
using LogAnalyzer.Kernel;
using NUnit.Framework;

namespace LogAnalyzer.Zip.Tests
{
	[TestFixture]
	public class ZipTests
	{
		private const string zipFileName = "Logs.zip";

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

		[Test]
		public void GetZippedLogs()
		{
			ZipDirectoryInfo zip = new ZipDirectoryInfo( new LogDirectoryConfigurationInfo( zipFileName, "", "" ), zipFileName, null );
			var files = zip.EnumerateFiles( "" ).ToList();

			foreach ( IFileInfo fileInfo in files )
			{
				Assert.NotNull( fileInfo );
				var reader = fileInfo.GetReader( new LogFileReaderArguments
				{
					Encoding = Encoding.GetEncoding( 1251 ),
					GlobalEntriesFilter = new DelegateFilter<LogEntry>( logEntry => true )
				} );

				var entries = reader.ReadEntireFile();
				Assert.Greater( entries.Count, 0 );
			}
		}
	}
}
