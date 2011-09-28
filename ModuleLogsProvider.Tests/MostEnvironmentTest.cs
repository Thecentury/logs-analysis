using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LogAnalyzer;
using LogAnalyzer.Config;
using LogAnalyzer.Kernel;
using LogAnalyzer.Operations;
using LogAnalyzer.Tests.Common;
using ModuleLogsProvider.Logging;
using ModuleLogsProvider.Logging.Mocks;
using ModuleLogsProvider.Logging.Most;
using ModuleLogsProvider.Logging.MostLogsServices;
using NUnit.Framework;

namespace ModuleLogsProvider.Tests
{
	[TestFixture]
	public class MostEnvironmentTest
	{
		private const string FirstFileName = @"L1";
		private const string SecondFileName = @"L2";

		[Description( "Одно сообщение" )]
		[Test]
		[TestCaseSource(typeof(MostEnvironmentTestDataSource), "TestCases" )]
		public void TestSingleMessageFromMost( TestCaseData data )
		{
			SendMessages( data, new LogMessageInfo { MessageType = "E", Message = "[E] [ 69] 24.05.2011 0:00:12	Message1", LoggerName = FirstFileName } );
		}

		[Description( "Несколько сообщений от одного логгера" )]
		[Test]
		[TestCaseSource( typeof( MostEnvironmentTestDataSource ), "TestCases" )]
		public void TestSerevalMessagesFromMost( TestCaseData data )
		{
			SendMessages( data,
				new LogMessageInfo { MessageType = "E", Message = "[E] [ 69] 24.05.2011 0:00:12	Message1", LoggerName = FirstFileName },
				new LogMessageInfo { MessageType = "E", Message = "[E] [ 69] 24.05.2011 0:00:13	Message2", LoggerName = FirstFileName }
				);
		}

		[Description( "Логирование из двух логгеров" )]
		[Test]
		[TestCaseSource( typeof( MostEnvironmentTestDataSource ), "TestCases" )]
		public void TestSeveralMessagesFromTwoLoggers( TestCaseData data )
		{
			SendMessages( data,
				new LogMessageInfo { MessageType = "E", Message = "[E] [ 69] 24.05.2011 0:00:12	Message1", LoggerName = FirstFileName },
				new LogMessageInfo { MessageType = "E", Message = "[E] [ 69] 24.05.2011 0:00:14	Message3", LoggerName = SecondFileName }
				);
		}

		private void SendMessages( TestCaseData data, params LogMessageInfo[] messages )
		{
			var loggerNames = messages.Select( m => m.LoggerName ).Distinct().ToList();
			MockLogsSourceService service = new MockLogsSourceService();
			MockTimer timer = new MockTimer();
			var serviceFactory = new MockLogSourceFactory( service );

			foreach ( var message in messages )
			{
				service.AddMessage( message );
			}

			LogAnalyzerConfiguration config = BuildConfig( timer, serviceFactory, data );
			MostEnvironment env = new MostEnvironment( config );

			LogAnalyzerCore core = new LogAnalyzerCore( config, env );
			core.Start();
			core.WaitForLoaded();

			timer.MakeRing();

			core.OperationsQueue.WaitAllRunningOperationsToComplete();

			ExpressionAssert.That( core, c => c.Directories.Count == 1 );

			var firstDir = core.Directories[0];
			Assert.That( firstDir, Is.Not.Null );
			ExpressionAssert.That( firstDir, d => d.Files.Count == loggerNames.Count );

			foreach ( var file in firstDir.Files )
			{
				string fileName = file.Name;
				var expectedMessagesCount = messages.Count( m => m.LoggerName == fileName );

				ExpressionAssert.That( file, f => f.LogEntries.Count == expectedMessagesCount );
			}
		}

		private LogAnalyzerConfiguration BuildConfig( ITimer timer, ILogSourceServiceFactory serviceFactory, TestCaseData data )
		{
			var config = LogAnalyzerConfiguration.CreateNew()
							.AddLogDirectory( "Dir1", "*", "Some directory 1" )
							.AddLogWriter( new DebugLogWriter() )
							.AcceptAllLogTypes()
							.RegisterInstance<ITimer>( timer )
							.RegisterInstance<ILogSourceServiceFactory>( serviceFactory )
							.RegisterInstance<OperationScheduler>( data.Scheduler )
							.RegisterInstance<IOperationsQueue>( data.OperationsQueue )
							.BuildConfig();

			return config;
		}
	}
}
