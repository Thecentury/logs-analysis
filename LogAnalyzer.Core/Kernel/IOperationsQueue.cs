using System;
using System.Threading;
using LogAnalyzer.Common;

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

	public static class OperationsQueueExtensions
	{
		public static void EnqueueOperation( this IOperationsQueue queue, ImpersonationContext context, Action action )
		{
			queue.EnqueueOperation( () =>
									{
										using ( context.ExecuteInContext() )
										{
											action();
										}
									} );
		}
	}
}
