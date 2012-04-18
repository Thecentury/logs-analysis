using System;
using System.Threading;
using LogAnalyzer.Extensions;
using System.Collections.Concurrent;
using System.Diagnostics;
using LogAnalyzer.Logging;
using LogAnalyzer.Properties;

namespace LogAnalyzer.Kernel
{
	public sealed class WorkerThreadOperationsQueue : IOperationsQueue
	{
		private int _totalOperationsCount;

		private readonly Logger _logger;
		private readonly BlockingCollection<IAsyncOperation> _operationsQueue;
		private readonly Thread _workerThread;

		private readonly PerformanceCounter _operationsCountCounter;

		/// <summary>
		/// Объект для синхронизации WaitAllRunningOperationsToComplete.
		/// </summary>
		private readonly object _operationsSync = new object();

		public WorkerThreadOperationsQueue() : this( Logger.Instance ) { }

		public WorkerThreadOperationsQueue( Logger logger )
		{
			if ( logger == null )
			{
				throw new ArgumentNullException( "logger" );
			}

			_operationsCountCounter = PerformanceCountersService.GetPendingOperationsCountCounter();

			this._logger = logger;

			_operationsQueue = new BlockingCollection<IAsyncOperation>( new ConcurrentQueue<IAsyncOperation>() );

			_workerThread = new Thread( MainThreadProc ) { IsBackground = true };

			if ( Settings.Default.DecreaseWorkerThreadsPriorities )
			{
				_workerThread.Priority = ThreadPriority.BelowNormal;
			}
			_workerThread.Name = "OperationsQueueThread";
			_workerThread.Start();
		}

		public void EnqueueOperation( Action action )
		{
			PerformanceCountersService.Increment( _operationsCountCounter );

			var operation = new DelegateOperation( action );
			_logger.WriteVerbose( "Core.EnqueueOperation: +{0}:{1} Count={2}", operation, operation.GetHashCode(), (_operationsQueue.Count + 1) );
			_operationsQueue.Add( operation );

			Interlocked.Increment( ref _totalOperationsCount );
		}

		/// <summary>
		/// Полное число операций, поставленных в очередь.
		/// </summary>
		public int TotalOperationsCount
		{
			get { return _totalOperationsCount; }
		}

		public Thread Thread
		{
			get { return _workerThread; }
		}

		public void WaitAllRunningOperationsToComplete()
		{
			if ( _operationsQueue.Count == 0 )
			{
				return;
			}

			lock ( _operationsSync )
			{
				while ( _operationsQueue.Count > 0 )
				{
					Monitor.Wait( _operationsSync );
				}
			}

			Condition.DebugAssert( _operationsQueue.Count == 0, "Число ожидающих операций должно быть равно 0." );
		}

		public bool IsSynchronous
		{
			get { return false; }
		}

		private void MainThreadProc( object state )
		{
			while ( true )
			{
				try
				{
					IAsyncOperation operation = _operationsQueue.Take();

					PerformanceCountersService.Decrement( _operationsCountCounter );

					_logger.DebugWriteVerbose( "Core.MainThreadProc: → {0}:{1}", operation, operation.GetHashCode() );
					operation.Execute();
					_logger.DebugWriteVerbose( "Core.MainThreadProc: ← {0}:{1} Count = {2}", operation, operation.GetHashCode(),
											 _operationsQueue.Count );

					// todo не удалить ли это в Release?
					// для WaitAllRunningOperationsToComplete
					lock ( _operationsSync )
					{
						Monitor.Pulse( _operationsSync );
					}
				}
				catch ( Exception exc )
				{
					string exceptionMessage = exc.ToString();

					_logger.WriteError( "WorkerThreadOperationQueue.MainThreadProcedure: Exc = " + exceptionMessage );

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
			bool shouldRethrow = !(exc is ThreadAbortException);
			return shouldRethrow;
		}
	}
}
