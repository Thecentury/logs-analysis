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
		[ExpectedException( typeof( InvalidOperationException ) )]
		[Test]
		public void Test_0_Directories()
		{
			var config = LogAnalyzerConfiguration.CreateNew();
			var operationsQueue = new WorkerThreadOperationsQueue( config.Logger );
			var env = new MockEnvironment( config, operationsQueue );

			LogAnalyzerCore core = new LogAnalyzerCore( config, env );
			core.Start();

			Assert.Fail( "Сюда мы прийти не должны." );
		}

		[ExpectedException( typeof( InvalidOperationException ) )]
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
			core.Start();

			Assert.Fail( "Сюда мы придти не должны." );
		}
	}
}
