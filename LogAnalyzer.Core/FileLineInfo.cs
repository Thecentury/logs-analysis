using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer
{
	public sealed class FileLineInfo
	{
		public string MethodName { get; set; }
		public string FileName { get; set; }
		public int LineNumber { get; set; }
	}
}
