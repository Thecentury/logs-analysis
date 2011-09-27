using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Kernel;
using NUnit.Framework;

namespace LogAnalyzer.Tests
{
	/// <summary>
	/// Проверка логирования в разных кодировках.
	/// </summary>
	[TestFixture]
	public class DifferentEncodingsTests : LoggingTestsBase
	{
		protected Func<IOperationsQueue> CreateOperationsQueue;

		[Test]
		public void TestLoggingInEncoding_ASCII()
		{
			TestLoggingInEncoding( Encoding.ASCII );
		}

		[Test]
		public void TestLoggingInEncoding_BigEndianUnicode()
		{
			TestLoggingInEncoding( Encoding.BigEndianUnicode );
		}

		[Test]
		public void TestLoggingInEncoding_Default()
		{
			TestLoggingInEncoding( Encoding.Default );
		}

		[Test]
		public void TestLoggingInEncoding_Unicode()
		{
			TestLoggingInEncoding( Encoding.Unicode );
		}

		[Test]
		public void TestLoggingInEncoding_UTF32()
		{
			TestLoggingInEncoding( Encoding.UTF32 );
		}

		[Test]
		public void TestLoggingInEncoding_UTF7()
		{
			TestLoggingInEncoding( Encoding.UTF7 );
		}

		[Test]
		public void TestLoggingInEncoding_UTF8()
		{
			TestLoggingInEncoding( Encoding.UTF8 );
		}

		[Test]
		public void TestLoggingInEncoding_1251()
		{
			TestLoggingInEncoding( Encoding.GetEncoding( 1251 ) );
		}

		[Test]
		public void TestLoggingInEncoding_1252()
		{
			TestLoggingInEncoding( Encoding.GetEncoding( 1252 ) );
		}

		private void TestLoggingInEncoding( Encoding encoding )
		{
			InitEnvironment( encoding, directoriesCount: 2 );
			WriteTestMessages();
		}
	}
}
