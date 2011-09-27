using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using LogAnalyzer.Extensions;

namespace LogAnalyzer
{
	internal interface IAsyncOperation
	{
		void Execute();
	}

	internal sealed class DelegateOperation : IAsyncOperation
	{
		private readonly Action action;

		public DelegateOperation( Action action )
		{
			if ( action == null )
				throw new ArgumentNullException( "action" );

			this.action = action;
		}

		[DebuggerStepThrough]
		public void Execute()
		{
			action();
		}

		public override string ToString()
		{
#if DEBUG
			string methodName = action.Method.Name;

			Type declaringType = action.Method.DeclaringType;
			bool isCompilerGenerated = declaringType.Name.Contains( "<>" );
			string className = isCompilerGenerated ? declaringType.DeclaringType.Name : declaringType.Name;

			return className + "." + methodName;
#else
			return base.ToString();
#endif
		}
	}
}
