using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace LogAnalyzer.Extensions
{
	public static class Condition
	{
		[Conditional( "DEBUG" )]
		[DebuggerStepThrough]
		public static void DebugAssert( bool condition, string message = null )
		{
			Assert( condition, message );
		}

		[DebuggerStepThrough]
		public static void Assert( bool condition, string message = null )
		{
			if ( !condition )
			{
				Fail( message );
			}
		}

		[DebuggerStepThrough]
		public static void Fail( string message = null )
		{
			if ( Debugger.IsAttached )
				Debugger.Break();

			throw new AssertionFailedException( message ?? "Assertion failed." );
		}

		[DebuggerStepThrough]
		public static void BreakIfAttached()
		{
			if ( Debugger.IsAttached )
			{
				Debugger.Break();
			}
		}
	}

	[Serializable]
	internal sealed class AssertionFailedException : Exception
	{
		public AssertionFailedException( string message ) : base( message ) { }
	}
}
