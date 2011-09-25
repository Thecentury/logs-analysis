using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace LogAnalyzer.Tests
{
	public sealed class ByteListWrapperStream : Stream
	{
		private readonly IList<byte> bytes = null;
		private readonly object sync = null;

		public ByteListWrapperStream( IList<byte> bytes, object sync )
		{
			if ( bytes == null )
				throw new ArgumentNullException( "bytes" );
			if ( sync == null )
				throw new ArgumentNullException( "sync" );

			this.bytes = bytes;
			this.sync = sync;
		}

		/// <summary>
		/// Content, for debugging purposes.
		/// </summary>
		public string Content
		{
			get
			{
				string content = Encoding.Unicode.GetString( bytes.ToArray() );
				return content;
			}
		}

		#region Stream implementation

		public override bool CanRead
		{
			get { return true; }
		}

		public override bool CanSeek
		{
			get { return true; }
		}

		public override bool CanWrite
		{
			get { return false; }
		}

		public override void Flush()
		{
			throw new NotSupportedException();
		}

		public override long Length
		{
			get
			{
				lock ( sync )
				{
					return bytes.Count;
				}
			}
		}

		private int position = 0;
		public override long Position
		{
			get
			{
				lock ( sync )
				{
					return position;
				}
			}
			set
			{
				lock ( sync )
				{
					position = (int)value;
				}
			}
		}

		public override int Read( byte[] buffer, int offset, int count )
		{
			lock ( sync )
			{
				int readCount = Math.Min( bytes.Count - position, count );

				for ( int i = 0; i < readCount; i++, position++ )
				{
					buffer[offset + i] = bytes[position];
				}

				return readCount;
			}
		}

		public override long Seek( long offset, SeekOrigin origin )
		{
			throw new NotSupportedException();
		}

		public override void SetLength( long value )
		{
			throw new NotSupportedException();
		}

		public override void Write( byte[] buffer, int offset, int count )
		{
			throw new NotSupportedException();
		}

		#endregion

	}
}
