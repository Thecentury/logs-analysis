using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Config;
using LogAnalyzer.Extensions;

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

		protected void InitEnvironment( Encoding encoding )
		{
			config = LogAnalyzerConfiguration.Create()
				.AddLogDirectory( "Dir1", "*", "Some directory 1" )
				.AddLogDirectory( "Dir2", "*", "Some directory 2" )
				.AddLogWriter( new DebugLogWriter() )
				.AcceptAllLogTypes()
				.BuildConfig();

			var operationsQueue = new WorkerThreadOperationsQueue( config.Logger );

			env = new MockEnvironment( config, operationsQueue );

			file1 = env.Directories.First().AddFile( "1" );
			file2 = env.Directories.First().AddFile( "2" );
			file3 = env.Directories.Second().AddFile( "3" );

			logger1 = new DeterminedTimeLogHelper( file1 );
			logger2 = new DeterminedTimeLogHelper( file2 );
			logger3 = new DeterminedTimeLogHelper( file3 );

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
