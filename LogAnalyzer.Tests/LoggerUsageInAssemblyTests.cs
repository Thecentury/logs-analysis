using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LogAnalyzer.Extensions;
using LogAnalyzer.LoggingTemplates;
using NUnit.Framework;

namespace LogAnalyzer.Tests
{
	[TestFixture]
	public class LoggerUsageInAssemblyTests
	{
		[Test]
		public void ShouldDeserializeFromXml()
		{
			LoadRegexes();
		}

		private List<Regex> LoadRegexes()
		{
			List<Regex> regexes = new List<Regex>();
			using ( var fs = new FileStream( @"LoggingTemplates\usages.xml", FileMode.Open, FileAccess.Read ) )
			{
				Stopwatch timer = Stopwatch.StartNew();

				var usages = LoggerUsageInAssembly.Deserialize( fs );

				foreach ( var usageInAssembly in usages )
				{
					foreach ( var usage in usageInAssembly.Usages )
					{
						regexes.Add( usage.Regex );
					}
				}

				var elapsed = timer.Elapsed;

				Console.WriteLine( "Loaded {0} regexes in {1} ms", regexes.Count, elapsed.TotalMilliseconds );
			}

			return regexes;
		}



		[TestCase( "OrderAviaProcesses clearing before=0 removed=0 after=0" )]
		public void FindRegexSequentially( string logEntry )
		{
			var regexes = LoadRegexes().OrderByDescending( r => r.ToString().Length ).ToList();

			Stopwatch timer = Stopwatch.StartNew();

			var allMatching = regexes.FirstOrDefault( r => r.Match( logEntry ).Success );

			var elapsed = timer.ElapsedMilliseconds;
			Console.WriteLine( "{0} ms: '{1}'", elapsed, allMatching );
		}

		[TestCase( "OrderAviaProcesses clearing before=0 removed=0 after=0" )]
		public void FindRegexParallel( string logEntry )
		{
			var regexes = LoadRegexes().OrderByDescending( r => r.ToString().Length ).ToList();

			Stopwatch timer = Stopwatch.StartNew();

			var allMatching = regexes.AsParallel().AsOrdered().FirstOrDefault( r => r.Match( logEntry ).Success );

			var elapsed = timer.ElapsedMilliseconds;
			Console.WriteLine( "{0} ms: '{1}'", elapsed, allMatching );
		}
	}
}
