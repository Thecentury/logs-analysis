using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogAnalyzer.Operations
{
	internal sealed class TaskOperationScheduler : OperationScheduler
	{
		protected override AsyncOperation CreateOperationCore( Action action )
		{
			Task task = new Task( action );
			TaskAsyncOperation op = new TaskAsyncOperation( task );
			return op;
		}
	}
}
