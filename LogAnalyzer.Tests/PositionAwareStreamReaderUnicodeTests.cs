using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LogAnalyzer.Misc;
using NUnit.Framework;

namespace LogAnalyzer.Tests
{
	[TestFixture]
	public class PositionAwareStreamReaderUnicodeTests
	{
		private MemoryStream _stream;
		private PositionAwareStreamReader _reader;
		private StreamWriter _writer;

		[SetUp]
		public void Setup()
		{
			_stream = new MemoryStream();
			_reader = new PositionAwareStreamReader( _stream, Encoding.Unicode );
			_writer = new StreamWriter( _stream, Encoding.Unicode );
			_writer.AutoFlush = true;
		}

		[Test]
		public void ShouldReadEmptyFile()
		{
			string str = _reader.ReadLine();
			Assert.IsNull( str );
		}

		[Test]
		public void ShouldReadOneLine()
		{
			string line = "line1";
			_writer.Write( line );
			_stream.Position = 0;

			string str = _reader.ReadLine();
			Assert.AreEqual( line, str );
		}

		[Test]
		public void ShouldReturnNullAtTheEndOfStream()
		{
			_writer.Write( "line" );
			_stream.Position = 0;

			_reader.ReadLine();

			Assert.IsNull( _reader.ReadLine() );
			Assert.IsNull( _reader.ReadLine() );
		}

		[Test]
		public void ShouldReadTwoLinesSeparatedByBackslashR()
		{
			string line1 = "line1";
			_writer.Write( line1 );
			_writer.Write( '\r' );
			string line2 = "line2";
			_writer.Write( line2 );

			_stream.Position = 0;

			string str1 = _reader.ReadLine();
			Assert.AreEqual( line1, str1 );

			string str2 = _reader.ReadLine();
			Assert.AreEqual( line2, str2 );

			Assert.IsNull( _reader.ReadLine() );
		}

		[Test]
		public void ShouldReadTwoLinesSeparatedWithBackslashRAndN()
		{
			string line1 = "line1";
			_writer.Write( line1 );
			_writer.Write( '\r' );
			_writer.Write( '\n' );
			string line2 = "line2";
			_writer.Write( line2 );

			_stream.Position = 0;

			string str1 = _reader.ReadLine();
			Assert.AreEqual( line1, str1 );

			string str2 = _reader.ReadLine();
			Assert.AreEqual( line2, str2 );

			Assert.IsNull( _reader.ReadLine() );
		}
	}
}
