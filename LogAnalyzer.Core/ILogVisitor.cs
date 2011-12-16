using System.Linq;
using System.Text;

namespace LogAnalyzer
{
	public interface ILogVisitor
	{
		void Visit( LogEntry logEntry );
		void Visit( LogFile logFile );
		void Visit( LogDirectory logDirectory );
		void Visit( LogAnalyzerCore core );
	}
}
