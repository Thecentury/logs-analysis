using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LogAnalyzer
{
	public interface IOperationsQueue
	{
		void EnqueueOperation( Action action );
		int TotalOperationsCount { get; }
		Thread Thread { get; }

		void WaitAllRunningOperationsToComplete();
	}
}
