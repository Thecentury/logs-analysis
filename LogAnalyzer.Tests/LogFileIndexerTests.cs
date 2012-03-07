using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Kernel;
using LogAnalyzer.Kernel.Parsers;
using LogAnalyzer.Tests.Mocks;
using NUnit.Framework;

namespace LogAnalyzer.Tests
{
	[TestFixture]
	public class LogFileIndexerTests
	{
		private MockLogRecordsSource _log;
		private MockFileInfo _file;
		private LogFileIndexer _indexer;
		private LogFileReaderArguments _args;

		[SetUp]
		public void Setup()
		{
			Encoding encoding = Encoding.Unicode;

			_log = new MockLogRecordsSource( "dir" );
			_file = new MockFileInfo( "1", "1", _log, encoding );

			_indexer = new LogFileIndexer();
			_args = new LogFileReaderArguments { Encoding = encoding, LineParser = new ManualLogLineParser() };
		}

		[Test]
		public void ShouldIndexEmptyFile()
		{
			var index = _indexer.BuildIndex( _file, _args );

			Assert.That( index.Records.Length == 0 );
		}

		[Test]
		public void ShouldIndexOneEntry()
		{
			_file.WriteInfo( "l1" );

			var index = _indexer.BuildIndex( _file, _args );

			Assert.That( index.Records.Length == 1 );
			Assert.That( index.Records[0].Offset == 0 );
		}

		[Test]
		public void ShouldIndexTwoEntries()
		{
			_file.WriteInfo( "l1" + Environment.NewLine );
			int len1 = _file.Length;
			_file.WriteInfo( "l2" );

			var index = _indexer.BuildIndex( _file, _args );
			var records = index.Records;

			Assert.That( records.Length == 2 );
			Assert.That( records[0].Offset == 0 );
			Assert.That( records[1].Offset, Is.EqualTo( len1 ) );
		}
	}
}
