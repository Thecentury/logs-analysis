using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using LogAnalyzer.Filters;
using LogAnalyzer.Kernel;
using NUnit.Framework;

namespace LogAnalyzer.Tests
{
	[TestFixture]
	public class LogLineParsingBenchmark
	{
		[Test]
		public void ReadLongFileWithMostLogLineParser()
		{
			var parser = new MostLogLineParser();
			ReadLongFileCore( parser );
		}

		[Test]
		public void ReadLongFileWithManualParser()
		{
			var parser = new ManualLogLineParser();
			ReadLongFileCore( parser );
		}

		private static void ReadLongFileCore( ILogLineParser parser )
		{
			var timer = Stopwatch.StartNew();
			const string path = @"C:\Logs\Security2.log";
			StreamLogFileReader reader = new StreamLogFileReader( new LogFileReaderArguments
																	{
																		LineParser = parser,
																		Encoding = Encoding.GetEncoding( 1251 ),
																		GlobalEntriesFilter = new DelegateFilter<LogEntry>( e => true )
																	}, new FileSystemStreamReader( new FileInfo( path ) ) );

			var entries = reader.ReadEntireFile();

			var elapsedTime = timer.ElapsedMilliseconds;
			Console.WriteLine( "Elapsed {0} ms", elapsedTime );
		}
	}
}
