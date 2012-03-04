using System.IO;
using System.Text;

namespace LogAnalyzer.Common
{
	public sealed class StreamReaderFactory : IStreamReaderFactory
	{
		public TextReader CreateReader( Stream stream, Encoding encoding )
		{
			return new StreamReader( stream, encoding );
		}
	}
}