using System.Collections;
using LogAnalyzer.Kernel;
using LogAnalyzer.Operations;

namespace ModuleLogsProvider.Tests.Auxilliary
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
