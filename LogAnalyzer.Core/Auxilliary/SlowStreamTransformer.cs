using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.Auxilliary
{
	public sealed class SlowStreamTransformer : ITransformer<Stream>
	{
		private readonly TimeSpan delay;

		public SlowStreamTransformer( TimeSpan delay )
		{
			this.delay = delay;
		}

		public Stream Transform( Stream obj )
		{
			if ( obj == null ) throw new ArgumentNullException( "obj" );
			return new SlowStream( obj, delay );
		}
	}
}
