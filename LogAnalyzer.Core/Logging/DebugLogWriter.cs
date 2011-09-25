using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace LogAnalyzer
{
	public sealed class DebugLogWriter : LogWriter
	{
		public override void WriteLine( string message )
		{
			Debug.WriteLine( message );
		}
	}
}
