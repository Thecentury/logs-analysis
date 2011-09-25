using System;

namespace LogAnalyzer
{
	public sealed class FileReadEventArgs : EventArgs
	{
		public int BytesReadSincePreviousCall { get; set; }
	}
}