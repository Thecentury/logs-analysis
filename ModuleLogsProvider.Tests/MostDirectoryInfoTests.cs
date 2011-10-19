using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awad.Eticket.ModuleLogsProvider.Types;
using LogAnalyzer;
using LogAnalyzer.Kernel;
using LogAnalyzer.Logging;
using LogAnalyzer.Tests.Common;
using ModuleLogsProvider.Logging;
using ModuleLogsProvider.Logging.Mocks;
using ModuleLogsProvider.Logging.Most;
using ModuleLogsProvider.Logging.MostLogsServices;
using NUnit.Framework;

namespace ModuleLogsProvider.Tests
{
	[Description( "Тесты для MostDirectoryInfo" )]
	[TestFixture]
	public class MostDirectoryInfoTests
	{
		[Description( "Файлы, возвращенные методом GetFileInfo класса MostDirectoryInfo, должны принадлежать списку файлов этого класса." )]
		[Test]
		public void TestGetTheSameFileTwice()
		{
			MockTimer timer = new MockTimer();
			var serverFactory = new MostServiceFactory<ILogSourceService>( new NetTcpBindingFactory(), MostServerUrls.Local.LogsSourceServiceUrl );
			
			WorkerThreadOperationsQueue queue = new WorkerThreadOperationsQueue();
			IErrorReportingService errorReportingService = new ErrorReportingService();

			MostLogNotificationSource source = new MostLogNotificationSource( timer, serverFactory, queue, errorReportingService );
			source.MessagesStorage.AppendMessages(
				new LogMessageInfo[]
					{
						new LogMessageInfo{ IndexInAllMessagesList = 0, LoggerName = "1", Message = "[E] [ 69] 24.05.2011 0:00:12	Message1", MessageType = MessageSeverity.Error},
						new LogMessageInfo{ IndexInAllMessagesList = 1, LoggerName = "2", Message = "[E] [ 69] 24.05.2011 0:00:12	Message1", MessageType = MessageSeverity.Error}
					}
				);

			MostDirectoryInfo dir = new MostDirectoryInfo( source );
			var files = dir.EnumerateFiles( "*" ).ToList();

			ExpressionAssert.That( files, f => f.Count == 2 );

			var file1 = dir.GetFileInfo( "1" );
			ExpressionAssert.That( files, f => f.Contains( file1 ) );

			var file2 = dir.GetFileInfo( "2" );
			ExpressionAssert.That( files, f => f.Contains( file2 ) );
		}
	}
}
