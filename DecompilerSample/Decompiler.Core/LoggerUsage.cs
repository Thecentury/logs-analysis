using System;

namespace Decompiler.Core
{
	[Serializable]
	public sealed class LoggerUsage
	{
		public string ClassName { get; set; }
		public string MethodName { get; set; }
		public string FileName { get; set; }
		public int LineNumber { get; set; }
		public string MessageSeverity { get; set; }
		public string FormatString { get; set; }

		public static readonly int LineNotFound = -1;
	}
}