using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.Kernel
{
	public interface ILogLineParser
	{
		bool TryExtractLogEntryData( string line, ref string type, ref int threadId, ref DateTime time, ref string text );
	}
}
