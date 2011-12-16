using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace LogAnalyzer.Tests
{
	[TestFixture]
	public class SaveToWriterTests
	{
		[Test]
		public void TestWriteLogEntryToStream()
		{
			LogEntry entry = new LogEntry( "-", 1, new DateTime( 2011, 12, 16, 14, 1, 43 ), "Logging started...", 1, null );

			DefaultLogEntryFormatter formatter = new DefaultLogEntryFormatter();

			string writtenText = formatter.Format( entry );

			Assert.That( writtenText, Is.EqualTo( "[-] [  1] 16.12.2011 14:01:43	Logging started..." + Environment.NewLine ) );
		}
	}
}
