using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.Operations
{
	internal sealed class SyncronousOperationScheduler : OperationScheduler
	{
		protected override AsyncOperation CreateOperationCore( Action action )
		{
			SyncronousAsyncOperation op = new SyncronousAsyncOperation( action );
			return op;
		}
	}
}
