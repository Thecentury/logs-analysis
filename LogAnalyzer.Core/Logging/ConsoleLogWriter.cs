using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace LogAnalyzer
{
	public sealed class ConsoleLogWriter : SingleThreadedLogWriter
	{
		protected override void OnNewMessage(string message)
		{
			Console.WriteLine(message);
		}
	}
}
