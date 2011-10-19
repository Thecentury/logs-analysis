using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Awad.Eticket.ModuleLogsProvider.Types;
using LogAnalyzer;
using LogAnalyzer.Config;
using LogAnalyzer.Kernel;
using LogAnalyzer.Operations;
using LogAnalyzer.Tests.Common;
using ModuleLogsProvider.Logging;
using ModuleLogsProvider.Logging.Mocks;
using ModuleLogsProvider.Logging.Most;
using ModuleLogsProvider.Logging.MostLogsServices;
using ModuleLogsProvider.Tests.Auxilliary;
using NUnit.Framework;
using TestCaseData = ModuleLogsProvider.Tests.Auxilliary.TestCaseData;

namespace ModuleLogsProvider.Tests
{
	[TestFixture]
	public class MostEnvironmentTest
	{
		private const string FirstFileName = @"L1";
		private const string SecondFileName = @"L2";

		[Description( "Одно сообщение" )]
		[Test]
		[TestCaseSource( typeof( MostEnvironmentTestDataSource ), "TestCases" )]
		public void TestSingleMessageFromMost( TestCaseData data )
		{
			SendMessages( data, new LogMessageInfo { MessageType = MessageSeverity.Error, Message = "[E] [ 69] 24.05.2011 0:00:12	Message1", LoggerName = FirstFileName } );
		}

		[Description( "Несколько сообщений от одного логгера" )]
		[Test]
		[TestCaseSource( typeof( MostEnvironmentTestDataSource ), "TestCases" )]
		public void TestSerevalMessagesFromMost( TestCaseData data )
		{
			SendMessages( data,
				new LogMessageInfo { MessageType = MessageSeverity.Error, Message = "[E] [ 69] 24.05.2011 0:00:12	Message1", LoggerName = FirstFileName },
				new LogMessageInfo { MessageType = MessageSeverity.Error, Message = "[E] [ 69] 24.05.2011 0:00:13	Message2", LoggerName = FirstFileName }
				);
		}

		[Description( "Логирование из двух логгеров" )]
		[Test]
		[TestCaseSource( typeof( MostEnvironmentTestDataSource ), "TestCases" )]
		public void TestSeveralMessagesFromTwoLoggers( TestCaseData data )
		{
			SendMessages( data,
				new LogMessageInfo { MessageType = MessageSeverity.Error, Message = "[E] [ 69] 24.05.2011 0:00:12	Message1", LoggerName = FirstFileName },
				new LogMessageInfo { MessageType = MessageSeverity.Error, Message = "[E] [ 69] 24.05.2011 0:00:14	Message3", LoggerName = SecondFileName }
				);
		}

		private void SendMessages( TestCaseData data, params LogMessageInfo[] messages )
		{
			addedFilesCount = 0;

			var loggerNames = messages.Select( m => m.LoggerName ).Distinct().ToList();
			MockLogsSourceService service = new MockLogsSourceService();
			MockTimer timer = new MockTimer();
			var serviceFactory = new MockLogSourceFactory( service );

			foreach ( var message in messages )
			{
				service.AddMessage( message );
			}

			MostLogAnalyzerConfiguration config = EnvironmentTestHelper.BuildConfig( timer, serviceFactory, data.Scheduler, data.OperationsQueue );
			MostEnvironment env = new MostEnvironment( config );

			LogAnalyzerCore core = new LogAnalyzerCore( config, env );
			core.Directories.First().Files.CollectionChanged += Files_CollectionChanged;
			core.Start();
			core.WaitForLoaded();

			timer.Invoke();

			if ( !data.OperationsQueue.IsSynchronous )
			{
				Thread.Sleep( 2000 );
			}

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

			core.Directories.First().Files.CollectionChanged -= Files_CollectionChanged;
			ExpressionAssert.That( firstDir, d => d.Files.Count == addedFilesCount );
		}

		private int addedFilesCount = 0;
		private void Files_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			if ( e.Action == NotifyCollectionChangedAction.Add )
			{
				addedFilesCount += e.NewItems.Count;
			}
		}
	}
}
