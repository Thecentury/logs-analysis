using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer
{
	public abstract class LogWriter
	{
		public abstract void WriteLine(string message);
	}
}
