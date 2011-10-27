using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Extensions;
using LogAnalyzer.Tests.Common;
using NUnit.Framework;

namespace LogAnalyzer.Tests
{
	[TestFixture]
	public class SingleDirectoryLoggingTest : LoggingTestsBase
	{
		[TestFixtureSetUp]
		public void Init()
		{
			InitEnvironment( Encoding.Unicode, directoriesCount: 1 );
		}

		[Timeout( 4000 )]
		[Test]
		public void TestSingleDirectoryLogging()
		{
			WriteTestMessages();

			core.OperationsQueue.WaitAllRunningOperationsToComplete();
			core.WaitForMergedEntriesCount( 4, timeout: 2000 ).AssertIsTrueOrFailWithMessage( "Превышено время ожидания." );

			core.MergedEntries.AssertAreSorted( LogEntryByDateComparer.Instance );

			ExpressionAssert.That( core, c => c.Directories.Count == 1 );
			ExpressionAssert.That( core, c => c.MergedEntries.Count == core.Directories[0].MergedEntries.Count );
		}
	}
}
