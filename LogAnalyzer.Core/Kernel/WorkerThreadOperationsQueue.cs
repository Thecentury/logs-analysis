using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using LogAnalyzer.Extensions;
using System.Collections.Concurrent;
using System.Diagnostics;
using LogAnalyzer.Kernel;

namespace LogAnalyzer
{
	public sealed class WorkerThreadOperationsQueue : IOperationsQueue
	{
		private int totalOperationsCount;

		private readonly Logger logger;
		private readonly BlockingCollection<IAsyncOperation> operationsQueue;
		private readonly Thread workerThread;

		private readonly PerformanceCounter operationsCountCounter;

		/// <summary>
		/// Объект для синхронизации WaitAllRunningOperationsToComplete.
		/// </summary>
		private readonly object operationsSync = new object();

		public WorkerThreadOperationsQueue( Logger logger )
		{
			if ( logger == null )
				throw new ArgumentNullException( "logger" );

			this.operationsCountCounter = PerformanceCountersService.GetPendingOperationsCountCounter();

			this.logger = logger;

			this.operationsQueue = new BlockingCollection<IAsyncOperation>( new ConcurrentQueue<IAsyncOperation>() );

			workerThread = new Thread( MainThreadProcedure );
			workerThread.IsBackground = true;
			workerThread.Name = "OperationsQueueThread";
			workerThread.Start();
		}

		public void EnqueueOperation( Action action )
		{
			PerformanceCountersService.Increment( operationsCountCounter );

			var operation = new DelegateOperation( action );
			logger.WriteVerbose( "Core.EnqueueOperation: +{0}:{1} Count={2}", operation, operation.GetHashCode(), (operationsQueue.Count + 1) );
			operationsQueue.Add( operation );

			Interlocked.Increment( ref totalOperationsCount );
		}

		/// <summary>
		/// Полное число операций, поставленных в очередь.
		/// </summary>
		public int TotalOperationsCount
		{
			get { return totalOperationsCount; }
		}

		public Thread Thread
		{
			get { return workerThread; }
		}

		public void WaitAllRunningOperationsToComplete()
		{
			if ( operationsQueue.Count == 0 )
				return;

			lock ( operationsSync )
				while ( operationsQueue.Count > 0 )
					Monitor.Wait( operationsSync );

			Condition.DebugAssert( operationsQueue.Count == 0, "Число ожидающих операций должно быть равно 0." );
		}

		private void MainThreadProcedure( object state )
		{
			// todo try-catch?
			while ( true )
			{
				IAsyncOperation operation = operationsQueue.Take();

				PerformanceCountersService.Decrement( operationsCountCounter );

				logger.DebugWriteVerbose( "Core.MainThreadProc: → {0}:{1}", operation, operation.GetHashCode() );
				operation.Execute();
				logger.DebugWriteVerbose( "Core.MainThreadProc: ← {0}:{1} Count = {2}", operation, operation.GetHashCode(), operationsQueue.Count );

				// todo не удалить ли это в Release?
				// для WaitAllRunningOperationsToComplete
				lock ( operationsSync )
					Monitor.Pulse( operationsSync );
			}
		}
	}
}
