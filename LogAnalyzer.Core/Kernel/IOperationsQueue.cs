using System;
using System.Threading;

namespace LogAnalyzer.Kernel
{
	public interface IOperationsQueue
	{
		void EnqueueOperation( Action action );
		int TotalOperationsCount { get; }
		Thread Thread { get; }

		void WaitAllRunningOperationsToComplete();

		bool IsSynchronous { get; }
	}
}
