using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LogAnalyzer.Collections;
using LogAnalyzer.Common;
using LogAnalyzer.Kernel;
using LogAnalyzer.Misc;

namespace LogAnalyzer
{
	public sealed class LogFileIndexer
	{
		public LogFileIndexer()
		{
		}

		public LogFileIndex BuildIndex( [NotNull] IFileInfo file, [NotNull] LogFileReaderArguments arguments )
		{
			if ( file == null )
			{
				throw new ArgumentNullException( "file" );
			}
			if ( arguments == null )
			{
				throw new ArgumentNullException( "arguments" );
			}

			using ( var stream = file.OpenStream() )
			{
				using ( var reader = new PositionAwareStreamReader( stream, arguments.Encoding ) )
				{
					LogFileNavigator navigator = new LogFileNavigator( file, arguments, new FuncStreamReaderFactory( ( s, e ) => reader ) );

					List<IndexRecord> records = new List<IndexRecord>();

					using ( var enumerator = navigator.ToForwardEnumerable().GetEnumerator() )
					{
						do
						{
							IndexRecord record = new IndexRecord { Offset = reader.SavedPosition };
							bool hasEntry = enumerator.MoveNext();
							if ( !hasEntry )
							{
								break;
							}

							records.Add( record );

						} while ( true );
					}

					IndexRecord[] recordsArray = records.ToArray();

					return new LogFileIndex( recordsArray, stream.Length );
				}
			}
		}
	}
}