using System;
using LogAnalyzer.Common;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.Extensions
{
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