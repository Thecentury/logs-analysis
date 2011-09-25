using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer
{
	public enum LogEntryAppendResult
	{
		Added,
		NotParsed,
		ExcludedByFilter
	}
}
