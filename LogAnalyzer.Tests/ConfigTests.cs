using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace LogAnalyzer.Tests
{
	[TestFixture]
	public class ConfigTests
	{
		[ExpectedException( typeof( ArgumentException ) )]
		[Test]
		public void Test_0_Directories()
		{
			var config = LogAnalyzerConfiguration.Create().BuildConfig();
			var operationsQueue = new WorkerThreadOperationsQueue( config.Logger );
			var env = new MockEnvironment( config, operationsQueue );

			Core core = new Core( config, env );
		}

		[ExpectedException( typeof( ArgumentException ) )]
		[Test]
		public void Test_0_EnabledDirectories()
		{
			var config = LogAnalyzerConfiguration.Create()
				.AddLogDirectory( "some_path", "*", "fake_dir" )
				.AsDisabled()
				.BuildConfig();

			var operationsQueue = new WorkerThreadOperationsQueue( config.Logger );
			var env = new MockEnvironment( config, operationsQueue );

			Core core = new Core( config, env );
		}
	}
}
