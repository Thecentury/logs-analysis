using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Config;
using LogAnalyzer.Extensions;
using LogAnalyzer.Kernel;
using LogAnalyzer.Tests.Helpers;
using LogAnalyzer.Tests.Mocks;

namespace LogAnalyzer.Tests
{
	public abstract class LoggingTestsBase
	{
		protected LogAnalyzerConfiguration config;
		protected MockEnvironment env;
		protected MockFileInfo file1;
		protected MockFileInfo file2;
		protected MockFileInfo file3;
		protected LogAnalyzerCore core;
		protected DeterminedTimeLogHelper logger1;
		protected DeterminedTimeLogHelper logger2;
		protected DeterminedTimeLogHelper logger3;
		protected TotalOperationsCountHelper opsCount;

		protected void InitEnvironment( Encoding encoding, int directoriesCount = 2 )
		{
			var configBuilder = LogAnalyzerConfiguration.CreateNew();
			configBuilder.AddLogDirectory( "Dir1", "*", "Some directory 1" );
			if ( directoriesCount > 1 )
			{
				configBuilder.AddLogDirectory( "Dir2", "*", "Some directory 2" );
			}

			foreach ( var dir in configBuilder.Directories )
			{
				dir.EncodingName = "utf-16";
			}

			config = configBuilder
				.AddLogWriter( new DebugLogWriter() )
				.AcceptAllLogTypes();

			var operationsQueue = new WorkerThreadOperationsQueue( config.Logger );

			env = new MockEnvironment( config, operationsQueue );

			file1 = env.MockDirectories.First().AddFile( "1" );
			file2 = env.MockDirectories.First().AddFile( "2" );

			logger1 = new DeterminedTimeLogHelper( file1 );
			logger2 = new DeterminedTimeLogHelper( file2 );

			if ( directoriesCount > 1 )
			{
				file3 = env.MockDirectories.Second().AddFile( "3" );
				logger3 = new DeterminedTimeLogHelper( file3 );
			}

			core = new LogAnalyzerCore( config, env );
			core.Start();
			core.WaitForLoaded();

			opsCount = new TotalOperationsCountHelper( core );
		}

		protected void WriteTestMessages()
		{
			logger1.WriteInfo( "1-1", 1 );
			opsCount.AssertOperationsIncreased();

			logger2.WriteInfo( "2-1", 2 );
			opsCount.AssertOperationsIncreased();

			logger1.WriteInfo( "1-3", 4 );
			opsCount.AssertOperationsIncreased();

			logger1.WriteInfo( "1-4", 3 );
			opsCount.AssertOperationsIncreased();
		}
	}
}
