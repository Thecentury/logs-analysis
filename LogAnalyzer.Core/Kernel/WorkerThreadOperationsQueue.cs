﻿using System;
using System.Threading;
using LogAnalyzer.Extensions;
using System.Collections.Concurrent;
using System.Diagnostics;
using LogAnalyzer.Logging;

namespace LogAnalyzer.Kernel
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

		public WorkerThreadOperationsQueue() : this( new Logger() ) { }

		public WorkerThreadOperationsQueue( Logger logger )
		{
			if ( logger == null )
				throw new ArgumentNullException( "logger" );

			operationsCountCounter = PerformanceCountersService.GetPendingOperationsCountCounter();

			this.logger = logger;

			operationsQueue = new BlockingCollection<IAsyncOperation>( new ConcurrentQueue<IAsyncOperation>() );

			workerThread = new Thread( MainThreadProcedure );
			workerThread.IsBackground = true;
			workerThread.Name = "OperationsQueueThread";
			workerThread.Start();
		}

		public void EnqueueOperation( Action action )
		{
			PerformanceCountersService.Increment( operationsCountCounter );

			var operation = new DelegateOperation( action );
			logger.WriteVerbose( "Core.EnqueueOperation: +{0}:{1} Count={2}", operation, operation.GetHashCode(), ( operationsQueue.Count + 1 ) );
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

		public bool IsSyncronous
		{
			get { return false; }
		}

		private void MainThreadProcedure( object state )
		{
			while ( true )
			{
				try
				{
					IAsyncOperation operation = operationsQueue.Take();

					PerformanceCountersService.Decrement( operationsCountCounter );

					logger.DebugWriteVerbose( "Core.MainThreadProc: → {0}:{1}", operation, operation.GetHashCode() );
					operation.Execute();
					logger.DebugWriteVerbose( "Core.MainThreadProc: ← {0}:{1} Count = {2}", operation, operation.GetHashCode(),
											 operationsQueue.Count );

					// todo не удалить ли это в Release?
					// для WaitAllRunningOperationsToComplete
					lock ( operationsSync )
						Monitor.Pulse( operationsSync );
				}
				catch ( Exception exc )
				{
					string exceptionMessage = exc.ToString();
			
					bool shouldRethrow = ShouldRethrow( exc );
					if ( shouldRethrow )
					{
						Condition.BreakIfAttached();
						throw;
					}
				}
			}
		}

		private bool ShouldRethrow( Exception exc )
		{
			bool shouldRethrow = !( exc is ThreadAbortException );
			return shouldRethrow;
		}
	}
}
