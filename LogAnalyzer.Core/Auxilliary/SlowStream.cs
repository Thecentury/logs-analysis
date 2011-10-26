using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace LogAnalyzer.Auxilliary
{
	internal sealed class SlowStream : Stream
	{
		private readonly Stream inner;
		private readonly TimeSpan delay;

		public SlowStream( Stream inner, TimeSpan delay )
		{
			if ( inner == null ) throw new ArgumentNullException( "inner" );
			this.inner = inner;
			this.delay = delay;
		}

		public override void Flush()
		{
			inner.Flush();
		}

		public override long Seek( long offset, SeekOrigin origin )
		{
			Sleep();
			return inner.Seek( offset, origin );
		}

		private void Sleep()
		{
			Thread.Sleep( delay );
		}

		public override void SetLength( long value )
		{
			inner.SetLength( value );
		}

		public override int Read( byte[] buffer, int offset, int count )
		{
			Sleep();
			return inner.Read( buffer, offset, count );
		}

		public override void Write( byte[] buffer, int offset, int count )
		{
			Sleep();
			inner.Write( buffer, offset, count );
		}

		public override bool CanRead
		{
			get { return inner.CanRead; }
		}

		public override bool CanSeek
		{
			get { return inner.CanSeek; }
		}

		public override bool CanWrite
		{
			get { return inner.CanWrite; }
		}

		public override long Length
		{
			get { return inner.Length; }
		}

		public override long Position
		{
			get { return inner.Position; }
			set { inner.Position = value; }
		}
	}
}
