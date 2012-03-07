using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using LogAnalyzer.Collections;
using LogAnalyzer.Common;
using LogAnalyzer.Kernel;
using LogAnalyzer.Kernel.Parsers;
using LogAnalyzer.Misc;

namespace LogAnalyzer.ConsoleApp
{
	class Program
	{
		private const string FileName = @"C:\Logs\Security2.!log!";

		static void Main( string[] args )
		{
			var arguments = new LogFileReaderArguments
			{
				Encoding = Encoding.UTF8,
				LineParser = new ManualLogLineParser()
			};


			ExecuteMergingNavigator( arguments );
			//ExecuteNavigator( arguments );
			//CalculateEntriesCount( arguments );
		}

		private static void ExecuteMergingNavigator( LogFileReaderArguments arguments )
		{
			var files = Directory.GetFiles( @"D:\MOST\AWAD\logs", "*.log", SearchOption.TopDirectoryOnly );
			var navigators = new IBidirectionalEnumerable<LogEntry>[files.Length];

			for ( int i = 0; i < files.Length; i++ )
			{
				var file = files[i];
				LogFile logFile = new LogFile( new FileSystemFileInfo( file ) );
				navigators[i] = new LogStreamNavigator( new StreamReader( file, Encoding.GetEncoding( 1251 ), false ), new ManualLogLineParser(), logFile );
			}

			MergingNavigator mergingNavigator = new MergingNavigator( navigators );
			var enumerator = mergingNavigator.GetEnumerator();

			int count = 0;
			LogEntry prev = null;
			IComparer<LogEntry> comparer = new LogEntryByDateComparer();

			Stopwatch timer = Stopwatch.StartNew();
			do
			{
				if ( !enumerator.MoveNext() )
				{
					break;
				}

				count++;

				//WriteEntry( enumerator.Current );
				if ( prev != null && comparer.Compare( prev, enumerator.Current ) > 0 )
				{
					break;
				}
				prev = enumerator.Current;

			} while ( true );

			timer.Stop();
			Console.WriteLine( "Count = {0}, elapsed {1} ms", count, timer.ElapsedMilliseconds );
		}

		private static void ExecuteNavigator( LogFileReaderArguments arguments )
		{
			Stopwatch timer = Stopwatch.StartNew();

			LogFileIndexer indexer = new LogFileIndexer();

			var index = indexer.BuildIndex( new FileSystemFileInfo( FileName ), arguments );

			timer.Stop();

			Console.WriteLine( "Elapsed {0} ms", timer.ElapsedMilliseconds );

			using ( var stream = new FileStream( FileName, FileMode.Open, FileAccess.Read ) )
			{
				IndexedLogStreamNavigator navigator = new IndexedLogStreamNavigator( stream, arguments.Encoding,
																					new ManualLogLineParser(), index );
				var enumerator = navigator.GetEnumerator();

				do
				{
					Console.Write( ">>> " );
					ConsoleKeyInfo key = Console.ReadKey();
					Console.WriteLine();

					if ( key.Key == ConsoleKey.N )
					{
						bool canMoveNext = enumerator.MoveNext();
						if ( !canMoveNext )
						{
							Console.WriteLine();
							Console.WriteLine( "Reached the end of file" );
							continue;
						}

						var entry = enumerator.Current;
						WriteEntry( entry );
					}
					else if ( key.Key == ConsoleKey.P )
					{
						bool canMoveBack = enumerator.MoveBack();
						if ( !canMoveBack )
						{
							Console.WriteLine();
							Console.WriteLine( "Reached the beginning of file" );
							continue;
						}

						var entry = enumerator.Current;
						WriteEntry( entry );
					}
					else if ( key.Key == ConsoleKey.Escape )
					{
						Console.WriteLine( "Exiting." );
						break;
					}
				} while ( true );
			}
		}

		private static void WriteEntry( LogEntry entry )
		{
			StringWriter writer = new StringWriter();
			entry.Accept( new LogSaveVisitor( writer, new DefaultLogEntryFormatter() ) );

			Console.WriteLine( writer.ToString() );
		}

		private static void CalculateEntriesCount( LogFileReaderArguments arguments )
		{
			LogFileNavigator navigator = new LogFileNavigator( new FileSystemFileInfo( FileName ), arguments,
															  new FuncStreamReaderFactory(
																( s, e ) => new PositionAwareStreamReader( s, e ) ) );
			int count = navigator.ToForwardEnumerable().Count();
			Console.WriteLine( "Count = {0}", count );
		}
	}
}
