using System;
using System.IO;
using System.Text;

namespace LogAnalyzer.Common
{
	public sealed class FuncStreamReaderFactory : IStreamReaderFactory
	{
		private readonly Func<Stream, Encoding, TextReader> _creater;

		public FuncStreamReaderFactory( Func<Stream, Encoding, TextReader> creater )
		{
			_creater = creater;
		}

		public TextReader CreateReader( Stream stream, Encoding encoding )
		{
			TextReader reader = _creater( stream, encoding );
			return reader;
		}
	}
}