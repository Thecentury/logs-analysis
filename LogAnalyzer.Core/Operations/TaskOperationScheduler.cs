using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LogAnalyzer.Properties;

namespace LogAnalyzer.Operations
{
	internal sealed class TaskOperationScheduler : OperationScheduler
	{
		protected override AsyncOperation CreateOperationCore( Action action )
		{
			Action wrapper = action;
			if ( Settings.Default.DecreaseWorkerThreadsPriorities )
			{
				wrapper = () =>
				{
					ThreadPriority priority = Thread.CurrentThread.Priority;
					Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
					try
					{
						action();
					}
					finally
					{
						Thread.CurrentThread.Priority = priority;
					}
				};
			}

			Task task = new Task( wrapper );
			TaskAsyncOperation op = new TaskAsyncOperation( task );
			return op;
		}
	}
}
