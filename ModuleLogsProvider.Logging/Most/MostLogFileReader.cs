using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer;
using LogAnalyzer.Kernel;

namespace ModuleLogsProvider.Logging.Most
{
	internal sealed class MostLogFileReader : LogFileReaderBase
	{
		private readonly OneFileMessages messages;

		public MostLogFileReader( LogFileReaderArguments args, OneFileMessages messages )
		{
			if ( args == null ) throw new ArgumentNullException( "args" );
			if ( messages == null ) throw new ArgumentNullException( "messages" );

			this.messages = messages;
			messages.SetLogFile( args.ParentLogFile );
		}

		public override IList<LogEntry> ReadToEnd( LogEntry lastAddedEntry )
		{
			int startingIndex = GetStartingIndex( lastAddedEntry );

			var result = messages.Entries.Skip( startingIndex ).ToList();
			return result;
		}

		private int GetStartingIndex( LogEntry lastAddedEntry )
		{
			int index = messages.Entries.IndexOf( lastAddedEntry ) + 1;
			return index;
		}
	}
}
