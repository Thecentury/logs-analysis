using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace LogAnalyzer.Misc
{
	public sealed class PositionAwareStreamReader : TextReader
	{
		private const int BufferLength = 1024;

		private readonly Stream _stream;
		private readonly Encoding _encoding;
		private readonly Decoder _decoder;
		private readonly byte[] _bytes;
		private readonly char[] _chars;
		private int _charPos;
		private int _charLen;
		private int _bytesPos;
		private int _bytesLen;
		private long _positionInFile;
		private bool _preambleWasRead;

		public PositionAwareStreamReader( [NotNull] Stream stream, [NotNull] Encoding encoding )
		{
			if ( stream == null )
			{
				throw new ArgumentNullException( "stream" );
			}
			if ( encoding == null )
			{
				throw new ArgumentNullException( "encoding" );
			}

			_stream = stream;
			if ( !_stream.CanSeek )
			{
				throw new ArgumentException( "Stream provided should be seekable." );
			}

			_encoding = encoding;
			_decoder = encoding.GetDecoder();
			_bytes = new byte[BufferLength];
			_chars = new char[_encoding.GetMaxCharCount( _bytes.Length )];
			_positionInFile = stream.Position;
		}

		public long PositionInFile
		{
			get { return _positionInFile; }
		}

		private void ReadPreamble()
		{
			if ( _preambleWasRead )
			{
				return;
			}

			_preambleWasRead = true;

			byte[] preamble = _encoding.GetPreamble();

			if ( _bytesLen < preamble.Length )
			{
				throw new InvalidOperationException( "Число прочитанных байтов меньше преамбулы кодировки." );
			}

			for ( int i = 0; i < preamble.Length; i++ )
			{
				byte b = _bytes[_bytesPos + i];
				if ( b != preamble[i] )
				{
					// это не преамбула
					return;
				}
			}

			// сдвигаем положение на длину преамбулы
			_positionInFile += preamble.Length;
			_bytesPos += preamble.Length;
		}

		public override void Close()
		{
			_stream.Dispose();
		}

		protected override void Dispose( bool disposing )
		{
			_stream.Dispose();
			base.Dispose( disposing );
		}

		#region Not implemented

		public override int Peek()
		{
			throw new NotImplementedException();
		}

		public override int Read()
		{
			throw new NotImplementedException();
		}

		public override int Read( char[] buffer, int index, int count )
		{
			throw new NotImplementedException();
		}

		public override int ReadBlock( char[] buffer, int index, int count )
		{
			throw new NotImplementedException();
		}

		public override string ReadToEnd()
		{
			throw new NotImplementedException();
		}

		#endregion

		private int ReadBuffer()
		{
			_charLen = 0;
			_charPos = 0;
			_bytesPos = 0;

			_bytesLen = _stream.Read( _bytes, 0, _bytes.Length );
			if ( _bytesLen == 0 )
			{
				return 0;
			}

			if ( !_preambleWasRead )
			{
				ReadPreamble();
			}

			_charLen = _decoder.GetChars( _bytes, _bytesPos, _bytesLen - _bytesPos, _chars, 0 );

			return _charLen;
		}

		public override string ReadLine()
		{
			if ( _charPos == _charLen )
			{
				int readToBuffer = ReadBuffer();
				if ( readToBuffer == 0 )
				{
					return null;
				}
			}

			var encoder = _encoding.GetEncoder();

			StringBuilder builder = null;

			while ( true )
			{
				char c = '\0';
				bool eolFound = false;
				int stringLength = 0;
				for ( int i = _charPos; i < _charLen; i++ )
				{
					c = _chars[i];
					if ( c == '\r' || c == '\n' )
					{
						eolFound = true;
						break;
					}

					stringLength++;
				}

				if ( builder == null )
				{
					builder = new StringBuilder( stringLength + 80 );
				}

				builder.Append( _chars, _charPos, stringLength );

				int bytesUsed = encoder.GetByteCount( _chars, _charPos, stringLength, flush: true );
				_positionInFile += bytesUsed;

				_charPos += stringLength;

				if ( eolFound )
				{
					// на символ '\r'
					_charPos++;

					if ( c == '\r' )
					{
						bool hasNextChar = _charPos < _charLen || ReadBuffer() > 0;
						if ( hasNextChar && _chars[_charPos] == '\n' )
						{
							_charPos++;
							bytesUsed = encoder.GetByteCount( _chars, _charPos, 1, flush: false );
							_positionInFile += bytesUsed;
						}
					}

					return builder.ToString();
				}

				int readToBuffer = ReadBuffer();
				if ( readToBuffer == 0 )
				{
					return builder.ToString();
				}
			}
		}
	}
}
