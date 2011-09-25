using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Config;
using LogAnalyzer.Extensions;
using NUnit.Framework;

namespace LogAnalyzer.Tests
{
	[TestFixture]
	public class RealDataTests
	{
		[Ignore]
		[Test]
		public void TestRealData()
		{
			const string encodingName = "windows-1251";
			LogAnalyzerConfiguration config =
							LogAnalyzerConfiguration.Create()
							.AddLogDirectory( @"C:\Logs\Logs1", "*", "Logs1" )
							.WithEncoding( encodingName )
							.AddLogDirectory( @"C:\Logs\Logs1", "*", "Logs2" )
							.WithEncoding( encodingName )
							.AddLogWriter( new DebugLogWriter() )
							.AddAcceptedType( MessageType.Error )
							.BuildConfig();

			FileSystemEnvironment env = new FileSystemEnvironment( config );

			Core core = new Core( config, env );
			core.Start();
			core.WaitForLoaded();

			core.MergedEntries.AssertAreSorted( LogEntryByDateComparer.Instance );
			var merged1 = core.Directories.First().MergedEntries;
			merged1.AssertAreSorted( LogEntryByDateComparer.Instance );
			var merged2 = core.Directories.Second().MergedEntries;
			merged2.AssertAreSorted( LogEntryByDateComparer.Instance );

			Assert.IsTrue( merged1.Count > 0 );
			Assert.IsTrue( merged2.Count > 0 );
			Assert.AreEqual( merged1.Count + merged2.Count, core.MergedEntries.Count );
		}
	}
}
