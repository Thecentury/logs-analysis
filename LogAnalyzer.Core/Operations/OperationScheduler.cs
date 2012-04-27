using System;

namespace LogAnalyzer.Operations
{
    public abstract class OperationScheduler
    {
        public AsyncOperation CreateOperation(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            var operation = CreateOperationCore(action);
            return operation;
        }

        public void StartNewOperation(Action action)
        {
            AsyncOperation operation = CreateOperation(action);
            operation.Start();
        }

        protected abstract AsyncOperation CreateOperationCore(Action action);

        private static readonly SyncronousOperationScheduler syncronousScheduler = new SyncronousOperationScheduler();
        public static OperationScheduler SyncronousScheduler
        {
            get { return syncronousScheduler; }
        }

        private static readonly TaskOperationScheduler taskScheduler = new TaskOperationScheduler();
        public static OperationScheduler TaskScheduler
        {
            get { return taskScheduler; }
        }
    }
}
