using System;
using LogAnalyzer.Common;
using LogAnalyzer.Operations;

namespace LogAnalyzer.Extensions
{
    public static class OperationsSchedulerExtensions
    {
        public static AsyncOperation CreateOperation(this OperationScheduler scheduler, ImpersonationContext context, Action action)
        {
            var operation = scheduler.CreateOperation(() =>
                                                          {
                                                              using (context.ExecuteInContext())
                                                              {
                                                                  action();
                                                              }
                                                          });

            return operation;
        }

        public static void StartNewOperation(this OperationScheduler scheduler, ImpersonationContext context, Action action)
        {
            var operation = scheduler.CreateOperation(context, action);
            operation.Start();
        }
    }
}