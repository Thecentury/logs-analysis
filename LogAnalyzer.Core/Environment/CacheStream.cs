using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LogAnalyzer
{
	internal sealed class CacheStream : Stream
	{
		private readonly Stream remoteStream;
		private readonly Stream cacheReadStream;
		private readonly Stream cacheWriteStream;
		private long actualCachedLength;

		public CacheStream( Stream remoteStream, Stream cacheReadStream, Stream cacheWriteStream, int initialPosition )
		{
			if ( remoteStream == null )
				throw new ArgumentNullException( "remoteStream" );
			if ( cacheReadStream == null )
				throw new ArgumentNullException( "cacheReadStream" );
			if ( cacheWriteStream == null )
				throw new ArgumentNullException( "cacheWriteStream" );

			this.remoteStream = remoteStream;
			this.cacheReadStream = cacheReadStream;
			this.cacheWriteStream = cacheWriteStream;

			remoteStream.Position = initialPosition;
			cacheWriteStream.Seek( 0, SeekOrigin.End );

			this.actualCachedLength = cacheReadStream.Length;
		}

		public override bool CanRead
		{
			get { return true; }
		}

		public override bool CanSeek
		{
			get { return false; }
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
			get { return remoteStream.Length; }
		}

		public override long Position
		{
			get
			{
				long position = Math.Max( remoteStream.Position, cacheReadStream.Position );
				return position;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		public override int Read( byte[] buffer, int offset, int count )
		{
			long readStartPosition = Position;
			long maxPositionAfterRead = readStartPosition + count;
			bool everythingIsCached = maxPositionAfterRead <= actualCachedLength;

			if ( !everythingIsCached )
			{
				int bytesToDownload = (int)(maxPositionAfterRead - actualCachedLength);
				// todo reuse this array?
				byte[] temp = new byte[bytesToDownload];

				remoteStream.Position = actualCachedLength;
				int downloadedBytesCount = remoteStream.Read( temp, 0, bytesToDownload );

				if ( downloadedBytesCount > 0 )
				{
					cacheWriteStream.Write( temp, 0, downloadedBytesCount );
					cacheWriteStream.Flush();
					actualCachedLength += downloadedBytesCount;
				}
			}

			cacheReadStream.Position = readStartPosition;
			int bytesRead = cacheReadStream.Read( buffer, offset, count );

			return bytesRead;
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

		protected override void Dispose( bool disposing )
		{
			if ( disposing )
			{
				remoteStream.Dispose();
				cacheReadStream.Dispose();
				cacheWriteStream.Dispose();
			}
		}
	}
}
