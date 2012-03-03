using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace LogAnalyzer.Misc
{
	/// <summary>Implements a <see cref="T:System.IO.TextReader" /> that reads characters from a byte stream in a particular encoding.</summary>
	/// <filterpriority>1</filterpriority>
	[ComVisible( true )]
	[Serializable]
	public class BCLPositionAwareStreamReader : TextReader
	{
		internal const int DefaultBufferSize = 1024;
		private const int DefaultFileStreamBufferSize = 4096;
		private const int MinBufferSize = 128;
		/// <summary>A <see cref="T:System.IO.StreamReader" /> object around an empty stream.</summary>
		/// <filterpriority>1</filterpriority>
		private bool _closable;
		private Stream _stream;
		private Encoding _encoding;
		private Decoder _decoder;
		private byte[] _byteBuffer;
		private char[] _charBuffer;
		private byte[] _preamble;
		private int _charPos;
		private int _charLen;
		private int _byteLen;
		private int _bytePos;
		private int _maxCharsPerBuffer;
		private bool _detectEncoding;
		private bool _checkPreamble;
		private bool _isBlocked;
		/// <summary>Gets the current character encoding that the current <see cref="T:System.IO.StreamReader" /> object is using.</summary>
		/// <returns>The current character encoding used by the current reader. The value can be different after the first call to any <see cref="Overload:System.IO.StreamReader.Read" /> method of <see cref="T:System.IO.StreamReader" />, since encoding autodetection is not done until the first call to a <see cref="Overload:System.IO.StreamReader.Read" /> method.</returns>
		/// <filterpriority>2</filterpriority>
		public virtual Encoding CurrentEncoding
		{
			get
			{
				return this._encoding;
			}
		}
		/// <summary>Returns the underlying stream.</summary>
		/// <returns>The underlying stream.</returns>
		/// <filterpriority>2</filterpriority>
		public virtual Stream BaseStream
		{
			get
			{
				return this._stream;
			}
		}
		internal bool Closable
		{
			get
			{
				return this._closable;
			}
		}
		/// <summary>Gets a value that indicates whether the current stream position is at the end of the stream.</summary>
		/// <returns>true if the current stream position is at the end of the stream; otherwise false.</returns>
		/// <exception cref="T:System.ObjectDisposedException">The underlying stream has been disposed.</exception>
		/// <filterpriority>1</filterpriority>
		public bool EndOfStream
		{
			[SecuritySafeCritical]
			get
			{
				if ( this._stream == null )
				{
					throw new ArgumentException( "Reader closed" );
				}
				if ( this._charPos < this._charLen )
				{
					return false;
				}
				int num = this.ReadBuffer();
				return num == 0;
			}
		}

		/// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamReader" /> class for the specified stream, with the specified character encoding.</summary>
		/// <param name="stream">The stream to be read. </param>
		/// <param name="encoding">The character encoding to use. </param>
		/// <exception cref="T:System.ArgumentException">
		///   <paramref name="stream" /> does not support reading. </exception>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="stream" /> or <paramref name="encoding" /> is null. </exception>
		public BCLPositionAwareStreamReader( Stream stream, Encoding encoding )
			: this( stream, encoding, true, 1024 )
		{
		}
		/// <summary>Initializes a new instance of the <see cref="T:System.IO.StreamReader" /> class for the specified stream, with the specified character encoding, byte order mark detection option, and buffer size.</summary>
		/// <param name="stream">The stream to be read. </param>
		/// <param name="encoding">The character encoding to use. </param>
		/// <param name="detectEncodingFromByteOrderMarks">Indicates whether to look for byte order marks at the beginning of the file. </param>
		/// <param name="bufferSize">The minimum buffer size. </param>
		/// <exception cref="T:System.ArgumentException">The stream does not support reading. </exception>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="stream" /> or <paramref name="encoding" /> is null. </exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="bufferSize" /> is less than or equal to zero. </exception>
		private BCLPositionAwareStreamReader( Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize )
		{
			if ( stream == null || encoding == null )
			{
				throw new ArgumentNullException( (stream == null) ? "stream" : "encoding" );
			}
			if ( !stream.CanRead )
			{
				throw new ArgumentException( "Argument_StreamNotReadable" );
			}
			if ( bufferSize <= 0 )
			{
				throw new ArgumentOutOfRangeException( "bufferSize" );
			}
			this.Init( stream, encoding, detectEncodingFromByteOrderMarks, bufferSize );
		}


		private void Init( Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize )
		{
			this._stream = stream;
			this._encoding = encoding;
			this._decoder = encoding.GetDecoder();
			if ( bufferSize < 128 )
			{
				bufferSize = 128;
			}
			this._byteBuffer = new byte[bufferSize];
			this._maxCharsPerBuffer = encoding.GetMaxCharCount( bufferSize );
			this._charBuffer = new char[this._maxCharsPerBuffer];
			this._byteLen = 0;
			this._bytePos = 0;
			this._detectEncoding = detectEncodingFromByteOrderMarks;
			this._preamble = encoding.GetPreamble();
			this._checkPreamble = (this._preamble.Length > 0);
			this._isBlocked = false;
			this._closable = true;
		}

		/// <summary>Closes the <see cref="T:System.IO.StreamReader" /> object and the underlying stream, and releases any system resources associated with the reader.</summary>
		/// <filterpriority>1</filterpriority>
		public override void Close()
		{
			this.Dispose( true );
		}
	
		/// <summary>Closes the underlying stream, releases the unmanaged resources used by the <see cref="T:System.IO.StreamReader" />, and optionally releases the managed resources.</summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
		protected override void Dispose( bool disposing )
		{
			try
			{
				if ( this.Closable && disposing && this._stream != null )
				{
					this._stream.Close();
				}
			}
			finally
			{
				if ( this.Closable && this._stream != null )
				{
					this._stream = null;
					this._encoding = null;
					this._decoder = null;
					this._byteBuffer = null;
					this._charBuffer = null;
					this._charPos = 0;
					this._charLen = 0;
					base.Dispose( disposing );
				}
			}
		}
		/// <summary>Allows a <see cref="T:System.IO.StreamReader" /> object to discard its current data.</summary>
		/// <filterpriority>2</filterpriority>
		public void DiscardBufferedData()
		{
			this._byteLen = 0;
			this._charLen = 0;
			this._charPos = 0;
			if ( this._encoding != null )
			{
				this._decoder = this._encoding.GetDecoder();
			}
			this._isBlocked = false;
		}

		/// <summary>Returns the next available character but does not consume it.</summary>
		/// <returns>An integer representing the next character to be read, or -1 if there are no characters to be read or if the stream does not support seeking.</returns>
		/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
		/// <filterpriority>1</filterpriority>
		[SecuritySafeCritical]
		public override int Peek()
		{
			if ( this._stream == null )
			{
				throw new ArgumentException( "Reader closed" );
			}
			if ( this._charPos == this._charLen && (this._isBlocked || this.ReadBuffer() == 0) )
			{
				return -1;
			}
			return (int)this._charBuffer[this._charPos];
		}

		/// <summary>Reads the next character from the input stream and advances the character position by one character.</summary>
		/// <returns>The next character from the input stream represented as an <see cref="T:System.Int32" /> object, or -1 if no more characters are available.</returns>
		/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
		/// <filterpriority>1</filterpriority>
		[SecuritySafeCritical]
		public override int Read()
		{
			if ( this._stream == null )
			{
				throw new ArgumentException( "Reader closed" );
			}
			if ( this._charPos == this._charLen && this.ReadBuffer() == 0 )
			{
				return -1;
			}
			int result = (int)this._charBuffer[this._charPos];
			this._charPos++;
			return result;
		}

		/// <summary>Reads a maximum of <paramref name="count" /> characters from the current stream into <paramref name="buffer" />, beginning at <paramref name="index" />.</summary>
		/// <returns>The number of characters that have been read, or 0 if at the end of the stream and no data was read. The number will be less than or equal to the <paramref name="count" /> parameter, depending on whether the data is available within the stream.</returns>
		/// <param name="buffer">When this method returns, contains the specified character array with the values between <paramref name="index" /> and (<paramref name="index + count - 1" />) replaced by the characters read from the current source. </param>
		/// <param name="index">The index of <paramref name="buffer" /> at which to begin writing. </param>
		/// <param name="count">The maximum number of characters to read. </param>
		/// <exception cref="T:System.ArgumentException">The buffer length minus <paramref name="index" /> is less than <paramref name="count" />. </exception>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="buffer" /> is null. </exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="index" /> or <paramref name="count" /> is negative. </exception>
		/// <exception cref="T:System.IO.IOException">An I/O error occurs, such as the stream is closed. </exception>
		/// <filterpriority>1</filterpriority>
		[SecuritySafeCritical]
		public override int Read( [In] [Out] char[] buffer, int index, int count )
		{
			if ( buffer == null )
			{
				throw new ArgumentNullException( "buffer" );
			}
			if ( index < 0 || count < 0 )
			{
				throw new ArgumentOutOfRangeException( (index < 0) ? "index" : "count" );
			}
			if ( buffer.Length - index < count )
			{
				throw new ArgumentException( "Argument_InvalidOffLen" );
			}
			if ( this._stream == null )
			{
				throw new ArgumentException( "Reader closed" );
			}
			int num = 0;
			bool flag = false;
			while ( count > 0 )
			{
				int num2 = this._charLen - this._charPos;
				if ( num2 == 0 )
				{
					num2 = this.ReadBuffer( buffer, index + num, count, out flag );
				}
				if ( num2 == 0 )
				{
					break;
				}
				if ( num2 > count )
				{
					num2 = count;
				}
				if ( !flag )
				{
					Buffer.BlockCopy( this._charBuffer, this._charPos * 2, buffer, (index + num) * 2, num2 * 2 );
					this._charPos += num2;
				}
				num += num2;
				count -= num2;
				if ( this._isBlocked )
				{
					break;
				}
			}
			return num;
		}
		/// <summary>Reads the stream from the current position to the end of the stream.</summary>
		/// <returns>The rest of the stream as a string, from the current position to the end. If the current position is at the end of the stream, returns the empty string("").</returns>
		/// <exception cref="T:System.OutOfMemoryException">There is insufficient memory to allocate a buffer for the returned string. </exception>
		/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
		/// <filterpriority>1</filterpriority>
		[SecuritySafeCritical]
		public override string ReadToEnd()
		{
			if ( this._stream == null )
			{
				throw new ArgumentException( "Reader closed" );
			}
			StringBuilder stringBuilder = new StringBuilder( this._charLen - this._charPos );
			do
			{
				stringBuilder.Append( this._charBuffer, this._charPos, this._charLen - this._charPos );
				this._charPos = this._charLen;
				this.ReadBuffer();
			}
			while ( this._charLen > 0 );
			return stringBuilder.ToString();
		}
		private void CompressBuffer( int n )
		{
			Buffer.BlockCopy( this._byteBuffer, n, this._byteBuffer, 0, this._byteLen - n );
			this._byteLen -= n;
		}

		private bool IsPreamble()
		{
			if ( !this._checkPreamble )
			{
				return this._checkPreamble;
			}
			int num = (this._byteLen >= this._preamble.Length) ? (this._preamble.Length - this._bytePos) : (this._byteLen - this._bytePos);
			int i = 0;
			while ( i < num )
			{
				if ( this._byteBuffer[this._bytePos] != this._preamble[this._bytePos] )
				{
					this._bytePos = 0;
					this._checkPreamble = false;
					break;
				}
				i++;
				this._bytePos++;
			}
			if ( this._checkPreamble && this._bytePos == this._preamble.Length )
			{
				this.CompressBuffer( this._preamble.Length );
				this._bytePos = 0;
				this._checkPreamble = false;
				this._detectEncoding = false;
			}
			return this._checkPreamble;
		}

		internal virtual int ReadBuffer()
		{
			this._charLen = 0;
			this._charPos = 0;
			if ( !this._checkPreamble )
			{
				this._byteLen = 0;
			}
			while ( true )
			{
				if ( this._checkPreamble )
				{
					int num = this._stream.Read( this._byteBuffer, this._bytePos, this._byteBuffer.Length - this._bytePos );
					if ( num == 0 )
					{
						break;
					}
					this._byteLen += num;
				}
				else
				{
					this._byteLen = this._stream.Read( this._byteBuffer, 0, this._byteBuffer.Length );
					if ( this._byteLen == 0 )
					{
						goto Block_5;
					}
				}
				this._isBlocked = (this._byteLen < this._byteBuffer.Length);
				if ( !this.IsPreamble() )
				{
					this._charLen += this._decoder.GetChars( this._byteBuffer, 0, this._byteLen, this._charBuffer, this._charLen );
				}
				if ( this._charLen != 0 )
				{
					goto Block_9;
				}
			}
			if ( this._byteLen > 0 )
			{
				this._charLen += this._decoder.GetChars( this._byteBuffer, 0, this._byteLen, this._charBuffer, this._charLen );
				goto IL_89;
			}
			goto IL_89;
		Block_5:
			return this._charLen;
		Block_9:
			return this._charLen;
		IL_89:
			return this._charLen;
		}
		private int ReadBuffer( char[] userBuffer, int userOffset, int desiredChars, out bool readToUserBuffer )
		{
			this._charLen = 0;
			this._charPos = 0;
			if ( !this._checkPreamble )
			{
				this._byteLen = 0;
			}
			int num = 0;
			readToUserBuffer = (desiredChars >= this._maxCharsPerBuffer);
			while ( true )
			{
				if ( this._checkPreamble )
				{
					int num2 = this._stream.Read( this._byteBuffer, this._bytePos, this._byteBuffer.Length - this._bytePos );
					if ( num2 == 0 )
					{
						break;
					}
					this._byteLen += num2;
				}
				else
				{
					this._byteLen = this._stream.Read( this._byteBuffer, 0, this._byteBuffer.Length );
					if ( this._byteLen == 0 )
					{
						return num;
					}
				}
				this._isBlocked = (this._byteLen < this._byteBuffer.Length);
				if ( !this.IsPreamble() )
				{
					this._charPos = 0;
					if ( readToUserBuffer )
					{
						num += this._decoder.GetChars( this._byteBuffer, 0, this._byteLen, userBuffer, userOffset + num );
						this._charLen = 0;
					}
					else
					{
						num = this._decoder.GetChars( this._byteBuffer, 0, this._byteLen, this._charBuffer, num );
						this._charLen += num;
					}
				}
				if ( num != 0 )
				{
					goto Block_11;
				}
			}
			if ( this._byteLen <= 0 )
			{
				return num;
			}
			if ( readToUserBuffer )
			{
				num += this._decoder.GetChars( this._byteBuffer, 0, this._byteLen, userBuffer, userOffset + num );
				this._charLen = 0;
				return num;
			}
			num = this._decoder.GetChars( this._byteBuffer, 0, this._byteLen, this._charBuffer, num );
			this._charLen += num;
			return num;
		Block_11:
			this._isBlocked &= (num < desiredChars);
			return num;
		}
		/// <summary>Reads a line of characters from the current stream and returns the data as a string.</summary>
		/// <returns>The next line from the input stream, or null if the end of the input stream is reached.</returns>
		/// <exception cref="T:System.OutOfMemoryException">There is insufficient memory to allocate a buffer for the returned string. </exception>
		/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
		/// <filterpriority>1</filterpriority>
		[SecuritySafeCritical]
		public override string ReadLine()
		{
			if ( this._stream == null )
			{
				throw new ArgumentException( "Reader closed" );
			}
			if ( this._charPos == this._charLen && this.ReadBuffer() == 0 )
			{
				return null;
			}
			StringBuilder stringBuilder = null;
			int num;
			char c;
			while ( true )
			{
				num = this._charPos;
				do
				{
					c = this._charBuffer[num];
					if ( c == '\r' || c == '\n' )
					{
						goto IL_44;
					}
					num++;
				}
				while ( num < this._charLen );
				num = this._charLen - this._charPos;
				if ( stringBuilder == null )
				{
					stringBuilder = new StringBuilder( num + 80 );
				}
				stringBuilder.Append( this._charBuffer, this._charPos, num );
				if ( this.ReadBuffer() <= 0 )
				{
					goto Block_11;
				}
			}
		IL_44:
			string result;
			if ( stringBuilder != null )
			{
				stringBuilder.Append( this._charBuffer, this._charPos, num - this._charPos );
				result = stringBuilder.ToString();
				goto IL_85;
			}
			result = new string( this._charBuffer, this._charPos, num - this._charPos );
			goto IL_85;
		Block_11:
			return stringBuilder.ToString();
		IL_85:
			this._charPos = num + 1;
			if ( c == '\r' && (this._charPos < this._charLen || this.ReadBuffer() > 0) && this._charBuffer[this._charPos] == '\n' )
			{
				this._charPos++;
			}
			return result;
		}
	}
}
