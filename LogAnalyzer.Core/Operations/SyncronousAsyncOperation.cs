using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.Operations
{
	internal sealed class SyncronousAsyncOperation : AsyncOperation
	{
		private readonly Action action;

		public SyncronousAsyncOperation( Action action )
		{
			if ( action == null )
				throw new ArgumentNullException( "action" );

			this.action = action;
		}

		public override void Start()
		{
			action();
		}
	}
}
