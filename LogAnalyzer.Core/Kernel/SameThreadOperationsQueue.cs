using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace LogAnalyzer.Kernel
{
	/// <summary>
	/// Очередь действий, выполняющая их синхронно.
	/// </summary>
	public sealed class SameThreadOperationsQueue : IOperationsQueue
	{
		private int totalOperationsPostedCount;

		public void EnqueueOperation( Action action )
		{
			action();

			totalOperationsPostedCount++;
		}

		public int TotalOperationsCount
		{
			get { return totalOperationsPostedCount; }
		}

		public Thread Thread
		{
			get { return Thread.CurrentThread; }
		}

		public void WaitAllRunningOperationsToComplete()
		{
			return;
		}

		public bool IsSynchronous
		{
			get { return true; }
		}
	}
}
