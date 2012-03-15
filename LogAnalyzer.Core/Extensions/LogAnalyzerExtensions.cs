using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using System.Diagnostics;

namespace LogAnalyzer.Extensions
{
	public static class LogAnalyzerExtensions
	{
		// todo если это нужно, то переписать на Rx
		//internal static LogEntriesAddWaiter GetWaiterForEntriesAdded( this Core core, int addedLogEntriesCount )
		//{
		//    BlockingCollection<LogEntry> collection = new BlockingCollection<LogEntry>( new ConcurrentQueue<LogEntry>() );

		//    EventHandler<LogEntryAddedEventArgs> handler = ( sender, e ) =>
		//    {
		//        collection.Add( e.AddedLogEntry );
		//    };

		//    core.LogEntryAdded += handler;

		//    Func<IList<LogEntry>> result = () =>
		//    {
		//        IList<LogEntry> addedEntries = collection.WaitForAdded( addedLogEntriesCount );
		//        core.LogEntryAdded -= handler;
		//        return addedEntries;
		//    };

		//    return new LogEntriesAddWaiter( result );
		//}

		//internal static IDisposable BeginWaitForEntriesAdded( this Core core, int addedLogEntriesCount )
		//{
		//    return core.GetWaiterForEntriesAdded( addedLogEntriesCount );
		//}

		public static bool WaitForMergedEntriesCount( this LogEntriesList core, int mergedLogEntriesCount, int timeout = 500 /*ms*/ )
		{
#if DEBUG
			Stopwatch timer = Stopwatch.StartNew();
#endif

			bool result = true;

			Debug.WriteLine( "MergedEntries.Count = " + core.MergedEntries.Count );

			if ( core.MergedEntries.Count >= mergedLogEntriesCount )
				return result;

			CountdownEvent evt = new CountdownEvent( mergedLogEntriesCount );
			EventHandler<LogEntryAddedEventArgs> handler = null;

			try
			{
				handler = ( sender, e ) =>
				{
					try
					{
						evt.Signal();
					}
					catch ( ObjectDisposedException ) { }
				};

				core.LogEntryAdded += handler;

				if ( core.MergedEntries.Count >= mergedLogEntriesCount )
					return result;

				result = evt.Wait( timeout );

#if DEBUG
				if ( !result )
				{
					Debug.WriteLine( "WaitForMergedEntriesCount: Elapsed " + timer.ElapsedMilliseconds + " ms" );
				}
#endif
				if ( core.MergedEntries.Count >= mergedLogEntriesCount )
					return true;

				return result;
			}
			finally
			{
				if ( handler != null )
				{
					core.LogEntryAdded -= handler;
				}
				evt.Dispose();
			}
		}

		public sealed class LogEntriesAddWaiter : IDisposable
		{
			private readonly Func<IList<LogEntry>> getEntriesFunc = null;
			internal LogEntriesAddWaiter( Func<IList<LogEntry>> getEntriesFunc )
			{
				this.getEntriesFunc = getEntriesFunc;
			}

			public IList<LogEntry> Wait()
			{
				return getEntriesFunc();
			}

			void IDisposable.Dispose()
			{
				Wait();
			}
		}
	}
}
