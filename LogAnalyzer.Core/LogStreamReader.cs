using System;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using LogAnalyzer.Kernel;

namespace LogAnalyzer
{
	public sealed class LogStreamReader
	{
		private readonly Stream _stream;
		private readonly Encoding _encoding;
		private readonly ILogLineParser _parser;

		public LogStreamReader( [NotNull] Stream stream, [NotNull] Encoding encoding, [NotNull] ILogLineParser parser )
		{
			if ( stream == null )
			{
				throw new ArgumentNullException( "stream" );
			}
			if ( encoding == null )
			{
				throw new ArgumentNullException( "encoding" );
			}
			if ( parser == null )
			{
				throw new ArgumentNullException( "parser" );
			}
			_stream = stream;
			_encoding = encoding;
			_parser = parser;
		}

		public LogEntry ReadTo( long to )
		{
			byte[] bytes = new byte[(int)(to - _stream.Position)];
			int bytesRead = _stream.Read( bytes, 0, bytes.Length );
			if ( bytesRead != bytes.Length )
			{
				throw new InvalidOperationException( "Read from stream less bytes than was expected" );
			}

			using ( MemoryStream ms = new MemoryStream( bytes ) )
			{
				using ( StreamReader reader = new StreamReader( ms, _encoding, false ) )
				{
					string line = reader.ReadLine();
					LogEntry logEntry = _parser.ParseHeader( line );

					while ( (line = reader.ReadLine()) != null )
					{
						logEntry.AppendLine( line );
					}

					return logEntry;
				}
			}
		}
	}
}