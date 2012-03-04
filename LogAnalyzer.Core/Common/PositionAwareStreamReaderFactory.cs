using System.IO;
using System.Text;
using LogAnalyzer.Misc;

namespace LogAnalyzer.Common
{
	public sealed class PositionAwareStreamReaderFactory : IStreamReaderFactory
	{
		public TextReader CreateReader( Stream stream, Encoding encoding )
		{
			return new PositionAwareStreamReader( stream, encoding );
		}
	}
}