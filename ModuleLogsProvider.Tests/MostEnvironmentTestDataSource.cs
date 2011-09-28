using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Kernel;
using LogAnalyzer.Operations;

namespace ModuleLogsProvider.Tests
{
	public static class MostEnvironmentTestDataSource
	{
		public static TestCaseData GetOneThreadData()
		{
			return new TestCaseData
			{
				OperationsQueue = new SameThreadOperationsQueue(),
				Scheduler = OperationScheduler.SyncronousScheduler
			};
		}

		public static TestCaseData GetWorkerThreadData()
		{
			return new TestCaseData
			{
				OperationsQueue = new WorkerThreadOperationsQueue(),
				Scheduler = OperationScheduler.TaskScheduler
			};
		}

		/// <summary>
		/// Данные для тестов.
		/// </summary>
		public static IEnumerable TestCases
		{
			get
			{
				yield return GetOneThreadData();
				yield return GetWorkerThreadData();
			}
		}
	}
}
