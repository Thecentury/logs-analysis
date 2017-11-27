using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LogAnalyzer.Config;
using LogAnalyzer.Kernel;
using LogAnalyzer.Tests.Mocks;
using NUnit.Framework;

namespace LogAnalyzer.Tests
{
	[TestFixture]
	public class ConfigTests
	{
		[Test()]
		public void Test_0_Directories()
		{
			var config = LogAnalyzerConfiguration.CreateNew();
			var operationsQueue = new WorkerThreadOperationsQueue( config.Logger );
			var env = new MockEnvironment( config, operationsQueue );

			LogAnalyzerCore core = new LogAnalyzerCore( config, env );
		    Assert.Throws(typeof(InvalidOperationException), () => core.Start());

			Assert.Fail( "Сюда мы прийти не должны." );
		}

		[Test]
		public void Test_0_EnabledDirectories()
		{
			var config = LogAnalyzerConfiguration.CreateNew()
				.AddLogDirectory(
					LogDirectoryConfigurationInfo.CreateNew()
						.WithPath( "some_path" ).WithFileNameFilter( "*" ).WithDisplayName( "fake_dir" )
						.AsDisabled() );

			var operationsQueue = new WorkerThreadOperationsQueue( config.Logger );
			var env = new MockEnvironment( config, operationsQueue );

			LogAnalyzerCore core = new LogAnalyzerCore( config, env );
		    Assert.Throws(typeof(InvalidOperationException), () => core.Start());

			Assert.Fail( "Сюда мы придти не должны." );
		}

		[Ignore("Local")]
		[TestCase( @"D:\Temp\2012 - Apr\Cache.logproj" )]
		public void TestIncludeLogFilesByCleanedNameDeserialization( string settingsFilePath )
		{
			var project = LogAnalyzerConfiguration.LoadFromFile( settingsFilePath );
		}
	}
}
