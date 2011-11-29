using System;
using System.Collections.Generic;
using System.IO;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.Tests.Mocks
{
	internal sealed class MockStreamProvider : IStreamProvider
	{
		private readonly List<byte> bytes;
		private readonly object sync;

		public MockStreamProvider( List<byte> bytes, object sync )
		{
			if ( bytes == null ) throw new ArgumentNullException( "bytes" );
			if ( sync == null ) throw new ArgumentNullException( "sync" );

			this.bytes = bytes;
			this.sync = sync;
		}

		public Stream OpenStream( int startPosition )
		{
			return new ByteListWrapperStream( bytes, sync );
		}
	}
}