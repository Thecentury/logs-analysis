using System.Diagnostics;

namespace LogAnalyzer
{
	[DebuggerDisplay( "Offset {Offset}" )]
	public struct IndexRecord
	{
		public long Offset { get; set; }
	}
}