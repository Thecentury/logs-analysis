using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Collections.Concurrent;
using LogAnalyzer.Extensions;
using System.Threading.Tasks;
using System.Diagnostics;
using NUnit.Framework;

namespace LogAnalyzer.Tests
{
	[TestFixture]
	public class LoggingTests : LoggingTestsBase
	{
		[SetUp]
		public void Init()
		{
			InitEnvironment( Encoding.Unicode );
		}

		[Test]
		public void TestSimpleMerge()
		{
			WriteTestMessages();

			core.OperationsQueue.WaitAllRunningOperationsToComplete();
			core.WaitForMergedEntriesCount( 4, timeout: 2000 ).AssertIsTrue( "Timeout" );

			core.MergedEntries.AssertAreSorted( LogEntryByDateComparer.Instance );
			core.Directories.First().MergedEntries.AssertAreSorted( LogEntryByDateComparer.Instance );

			Assert.AreEqual( core.MergedEntries.Count, core.Directories.First().MergedEntries.Count );
		}

		/// <summary>
		/// Добавление файла во время работы.
		/// </summary>
		[Test]
		public void TestDynamicAddFile()
		{
			var file3 = env.Directories.First().AddFile( "3" );
			file3.WriteInfo( "test" );

			core.WaitForMergedEntriesCount( 1, timeout: 1500 ).AssertIsTrue( "Timeout" );
		}

		/// <summary>
		/// Логирование из 2 директорий.
		/// </summary>
		[Test]
		public void TestSeveralDirectories()
		{
			logger1.WriteInfo( "1", 1 );
			logger2.WriteInfo( "4", 4 );
			logger3.WriteInfo( "3", 3 );
			logger1.WriteInfo( "2", 2 );
			logger2.WriteInfo( "7", 7 );
			logger3.WriteInfo( "6", 6 );
			logger1.WriteInfo( "5", 5 );

			core.WaitForMergedEntriesCount( 7, 2000 ).AssertIsTrue( "Timeout" );
			core.MergedEntries.AssertAreSorted( LogEntryByDateComparer.Instance );
			core.Directories.First().MergedEntries.AssertAreSorted( LogEntryByDateComparer.Instance );
			core.Directories.Second().MergedEntries.AssertAreSorted( LogEntryByDateComparer.Instance );
		}

		/// <summary>
		/// Записи в логах с одинаковым временем.
		/// </summary>
		[Test]
		public void TestRecordsWithEqualTime()
		{
			logger1.WriteInfo( "11", 1 );
			logger2.WriteInfo( "21", 1 );
			logger3.WriteInfo( "31", 1 );

			logger1.WriteInfo( "12", 2 );
			logger2.WriteInfo( "22", 2 );
			logger3.WriteInfo( "32", 2 );

			logger1.WriteInfo( "13", 3 );

			core.WaitForMergedEntriesCount( 7, 2000 ).AssertIsTrue( "Timeout" );

			core.MergedEntries.AssertAreSorted( LogEntryByDateComparer.Instance );
			core.Directories.First().MergedEntries.AssertAreSorted( LogEntryByDateComparer.Instance );
			core.Directories.Second().MergedEntries.AssertAreSorted( LogEntryByDateComparer.Instance );

			Assert.AreEqual( 5, core.Directories.First().MergedEntries.Count );
			Assert.AreEqual( 2, core.Directories.Second().MergedEntries.Count );
		}

		const int messagesCount = 10;

		[Test]
		public void TestMultiThreadedLogging()
		{
			Task task1 = StartNewLoggingTask( logger1 );
			Task task2 = StartNewLoggingTask( logger2 );

			Task.WaitAll( task1, task2 );

			Thread.Sleep( 300 );

			core.MergedEntries.AssertAreSorted( LogEntryByDateComparer.Instance );
			Assert.AreEqual( 2 * messagesCount, core.MergedEntries.Count );
		}

		private Task StartNewLoggingTask( DeterminedTimeLogHelper logger )
		{
			Task task = Task.Factory.StartNew( () =>
			{
				Random rnd = new Random( logger.GetHashCode() );

				for ( int i = 0; i < messagesCount; i++ )
				{
					logger.WriteInfo( i.ToString(), i );
					Thread.Sleep( rnd.Next( 50, 150 ) );
				}
			} );

			return task;
		}
	}
}
