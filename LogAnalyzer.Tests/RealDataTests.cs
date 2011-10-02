using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Config;
using LogAnalyzer.Extensions;
using LogAnalyzer.Kernel;
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
				LogAnalyzerConfiguration.CreateNew()
					.AddLogDirectory( new LogDirectoryConfigurationInfo
										{
											Path = @"C:\Logs\Logs1",
											FileNameFilter = "*",
											DisplayName = "Logs1",
											EncodingName = encodingName
										} )
					.AddLogDirectory( new LogDirectoryConfigurationInfo
										{
											Path = @"C:\Logs\Logs2",
											FileNameFilter = "*",
											DisplayName = "Logs2",
											EncodingName = encodingName
										} )
					.AddLogWriter( new DebugLogWriter() )
					.AddLoggerAcceptedMessageType( MessageType.Error );

			FileSystemEnvironment env = new FileSystemEnvironment( config );

			LogAnalyzerCore core = new LogAnalyzerCore( config, env );
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
