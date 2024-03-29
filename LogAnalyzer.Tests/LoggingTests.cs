﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Collections.Concurrent;
using LogAnalyzer.Extensions;
using System.Threading.Tasks;
using System.Diagnostics;
using LogAnalyzer.Tests.Mocks;
using NUnit.Framework;

namespace LogAnalyzer.Tests
{
	[Timeout( 6000 )]
	[TestFixture]
	public class LoggingTests : LoggingTestsBase
	{
		[SetUp]
		public void Init()
		{
			InitEnvironment( Encoding.Unicode, directoriesCount: 2 );
		}

		[Test]
		public void TestSimpleMerge()
		{
			WriteTestMessages();

			core.OperationsQueue.WaitAllRunningOperationsToComplete();
			core.WaitForMergedEntriesCount( 4, timeout: 2000 ).AssertIsTrueOrFailWithMessage( "Превышено время ожидания." );

			core.MergedEntries.AssertIsSorted( LogEntryByDateAndIndexComparer.Instance );
			core.Directories.First().MergedEntries.AssertIsSorted( LogEntryByDateAndIndexComparer.Instance );

			Assert.AreEqual( core.MergedEntries.Count, core.Directories.First().MergedEntries.Count );
		}

		/// <summary>
		/// Добавление файла во время работы.
		/// </summary>
		[Test]
		public void TestDynamicAddFile()
		{
			var file3 = env.MockDirectories.First().AddFile( "3" );
			file3.WriteInfo( "test" );

			core.WaitForMergedEntriesCount( 1, timeout: 1500 ).AssertIsTrueOrFailWithMessage( "Истекло время ожидания." );
		}

		/// <summary>
		/// Логирование из 2 директорий.
		/// </summary>
		[Test]
		[Timeout( 16000 )]
		public void TestSeveralDirectories()
		{
			logger1.WriteInfo( "1", 1 );
			logger2.WriteInfo( "4", 4 );
			logger3.WriteInfo( "3", 3 );
			logger1.WriteInfo( "2", 2 );
			logger2.WriteInfo( "7", 7 );
			logger3.WriteInfo( "6", 6 );
			logger1.WriteInfo( "5", 5 );

			core.WaitForMergedEntriesCount( 7, 6000 ).AssertIsTrueOrFailWithMessage( "Истекло время ожидания." );
			core.MergedEntries.AssertIsSorted( LogEntryByDateAndIndexComparer.Instance );
			core.Directories.First().MergedEntries.AssertIsSorted( LogEntryByDateAndIndexComparer.Instance );
			core.Directories.Second().MergedEntries.AssertIsSorted( LogEntryByDateAndIndexComparer.Instance );
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

			core.WaitForMergedEntriesCount( 7, 2000 ).AssertIsTrueOrFailWithMessage( "Timeout" );

			core.MergedEntries.AssertIsSorted( LogEntryByDateAndIndexComparer.Instance );
			core.Directories.First().MergedEntries.AssertIsSorted( LogEntryByDateAndIndexComparer.Instance );
			core.Directories.Second().MergedEntries.AssertIsSorted( LogEntryByDateAndIndexComparer.Instance );

			Assert.AreEqual( 5, core.Directories.First().MergedEntries.Count );
			Assert.AreEqual( 2, core.Directories.Second().MergedEntries.Count );
		}

		const int MessagesCount = 10;

		[Test]
		public void TestMultiThreadedLogging()
		{
			Task task1 = StartNewLoggingTask( logger1 );
			Task task2 = StartNewLoggingTask( logger2 );

			Task.WaitAll( task1, task2 );

			Thread.Sleep( 3000 );

			core.MergedEntries.AssertIsSorted( LogEntryByDateAndIndexComparer.Instance );
			Assert.AreEqual( 2 * MessagesCount, core.MergedEntries.Count );
		}

		private Task StartNewLoggingTask( DeterminedTimeLogHelper logger )
		{
			Task task = Task.Factory.StartNew( () =>
			{
				Random rnd = new Random( logger.GetHashCode() );

				for ( int i = 0; i < MessagesCount; i++ )
				{
					logger.WriteInfo( i.ToString(), i );
					Thread.Sleep( rnd.Next( 50, 150 ) );
				}
			} );

			return task;
		}
	}
}
