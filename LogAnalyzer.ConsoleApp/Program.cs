using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using LogAnalyzer.Collections;
using LogAnalyzer.Kernel;
using LogAnalyzer.Misc;

namespace LogAnalyzer.ConsoleApp
{
	class Program
	{
		static void Main( string[] args )
		{
			var arguments = new LogFileReaderArguments
			{
				Encoding = Encoding.UTF8,
				LineParser = new ManualLogLineParser()
			};

			LogFileNavigator navigator = new LogFileNavigator( new FileSystemFileInfo( @"C:\Logs\Security2.!log!" ), arguments,
				new FuncStreamReaderFactory( ( s, e ) => new PositionAwareStreamReader( s, e ) ) );

			Stopwatch timer = Stopwatch.StartNew();

			int count = navigator.ToForwardEnumerable().Count();

			timer.Stop();

			Console.WriteLine( "Count = {0}", count );
			Console.WriteLine( "Elapsed {0} ms", timer.ElapsedMilliseconds );
		}
	}
}
