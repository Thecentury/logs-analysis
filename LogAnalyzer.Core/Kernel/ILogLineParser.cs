﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.Kernel
{
	public interface ILogLineParser
	{
		bool TryExtractLogEntryData( string line, out string type, out int threadId, out DateTime time, out string text );
	}
}
