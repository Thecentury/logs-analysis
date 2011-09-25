using System.Text;

namespace LogAnalyzer.Kernel
{
	public sealed class LogFileReaderArguments
	{
		public Logger Logger { get; set; }
		public Encoding Encoding { get; set; }
		public LogFile ParentLogFile { get; set; }
	}
}
