using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Kernel;
using LogAnalyzer.Operations;

namespace ModuleLogsProvider.Tests
{
	public sealed class TestCaseData
	{
		public IOperationsQueue OperationsQueue { get; set; }
		public OperationScheduler Scheduler { get; set; }

		public override string ToString()
		{
			return OperationsQueue.GetType().Name + ", " + Scheduler.GetType().Name;
		}
	}
}
