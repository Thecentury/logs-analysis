using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogAnalyzer.Operations
{
	internal sealed class TaskAsyncOperation : AsyncOperation
	{
		private readonly Task task;

		public TaskAsyncOperation(Task task)
		{
			if (task == null) 
				throw new ArgumentNullException("task");

			this.task = task;
		}

		public override void Start()
		{
			task.Start();
		}
	}
}
