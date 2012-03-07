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
		private readonly LogFile _parentFile;

		public LogStreamNavigator( [NotNull] TextReader reader, [NotNull] ILogLineParser parser, LogFile parentFile = null )
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
			_parentFile = parentFile;
		}

		public IBidirectionalEnumerator<LogEntry> GetEnumerator()
		{
			return new LogStreamEnumerator( _reader, _parser, _parentFile, disposeReader: false );
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