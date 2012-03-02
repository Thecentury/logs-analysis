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
		const string Path = @"C:\Logs\Security2.log";

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

		[Test]
		public void SimplyReadFile()
		{
			RawRead( 81920, FileOptions.SequentialScan );
		}

		private static void RawRead( int buffer, FileOptions options )
		{
			var timer = Stopwatch.StartNew();

			using ( var fs = new FileStream( Path, FileMode.Open, FileAccess.Read, FileShare.Read, buffer, options ) )
			{
				using ( StreamReader reader = new StreamReader( fs, Encoding.GetEncoding( 1251 ) ) )
				{
					while ( reader.ReadLine() != null )
					{
					}
				}
			}

			Console.WriteLine( "Elapsed {0} ms", timer.ElapsedMilliseconds );
		}

		private static void ReadLongFileCore( ILogLineParser parser )
		{
			var timer = Stopwatch.StartNew();
			StreamLogFileReader reader = new StreamLogFileReader( new LogFileReaderArguments
																	{
																		LineParser = parser,
																		Encoding = Encoding.GetEncoding( 1251 ),
																		GlobalEntriesFilter = new DelegateFilter<LogEntry>( e => true )
																	}, new FileSystemStreamProvider( new FileInfo( Path ) ) );

			var entries = reader.ReadEntireFile();

			var elapsedTime = timer.ElapsedMilliseconds;
			Console.WriteLine( "Elapsed {0} ms", elapsedTime );
		}
	}
}
