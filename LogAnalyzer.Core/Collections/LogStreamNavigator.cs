using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.Collections
{
	public sealed class LogStreamNavigator : IBidirectionalEnumerable<LogEntry>
	{
		private readonly TextReader _reader;
		private readonly ILogLineParser _parser;

		public LogStreamNavigator( [NotNull] TextReader reader, [NotNull] ILogLineParser parser )
		{
			if ( reader == null )
			{
				throw new ArgumentNullException( "reader" );
			}
			if ( parser == null )
			{
				throw new ArgumentNullException( "parser" );
			}

			_reader = reader;
			_parser = parser;
		}

		public IBidirectionalEnumerator<LogEntry> GetEnumerator()
		{
			return new LogStreamEnumerator( _reader, _parser, parentFile: null, disposeReader: false );
		}

		IEnumerator<LogEntry> IEnumerable<LogEntry>.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}