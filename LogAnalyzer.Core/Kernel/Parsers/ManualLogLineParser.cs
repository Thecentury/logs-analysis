using System;

namespace LogAnalyzer.Kernel.Parsers
{
	public sealed class ManualLogLineParser : ILogLineParser
	{
		public bool TryExtractLogEntryData( string line, ref string type, ref int threadId, ref DateTime time, ref string text )
		{
			if ( line.Length == 0 )
			{
				return false;
			}
			if ( line[0] != '[' )
			{
				return false;
			}

			type = line.Substring( 1, 1 );

			int tidOpeningBracketIndex = line.IndexOf( '[', 3 );
			if ( tidOpeningBracketIndex < 0 )
			{
				return false;
			}

			int tidClosingBracketIndex = line.IndexOf( ']', tidOpeningBracketIndex + 1 );
			if ( tidClosingBracketIndex < 0 )
			{
				return false;
			}

			string tidString = line.Substring( tidOpeningBracketIndex + 1, tidClosingBracketIndex - tidOpeningBracketIndex - 1 ).Trim();
			threadId = Int32.Parse( tidString );

			const int minDateLength = 20;
			int tabIndex = line.IndexOf( '\t', tidClosingBracketIndex + minDateLength );
			if ( tabIndex < 0 )
			{
				return false;
			}

			string dateString = line.Substring( tidClosingBracketIndex + 2, tabIndex - tidClosingBracketIndex - 2 );
			time = MostLogLineParser.ParseDate( dateString );

			text = line.Substring( tabIndex + 1, line.Length - tabIndex - 1 );

			return true;
		}
	}
}