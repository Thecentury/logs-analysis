using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace LogAnalyzer
{
	// todo не отнаследовать ли от IOException?
	[Serializable]
	internal class LogAnalyzerIOException : LogAnalyzerException
	{
		public LogAnalyzerIOException() : base() { }
		public LogAnalyzerIOException( string message ) : base( message ) { }
		public LogAnalyzerIOException( string message, Exception inner ) : base( message, inner ) { }
		protected LogAnalyzerIOException( SerializationInfo info, StreamingContext context ) : base( info, context ) { }
	}
}
