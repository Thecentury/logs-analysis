using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Tests;

namespace TestConsoleApp
{
	class Program
	{
		static void Main( string[] args )
		{
			var benchmark = new LogLineParsingBenchmark();
			//benchmark.ReadLongFileWithMostLogLineParser();
			benchmark.ReadLongFileWithManualParser();
			//benchmark.SimplyReadFile();
		}
	}
}
