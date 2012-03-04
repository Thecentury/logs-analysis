using System.IO;
using System.Text;

namespace LogAnalyzer.Common
{
	public interface IStreamReaderFactory
	{
		TextReader CreateReader( Stream stream, Encoding encoding );
	}
}