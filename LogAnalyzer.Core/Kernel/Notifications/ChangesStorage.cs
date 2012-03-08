using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;

namespace LogAnalyzer.Kernel.Notifications
{
	internal sealed class ChangesStorage
	{
		private readonly Dictionary<string, List<EventArgs>> _events = new Dictionary<string, List<EventArgs>>();
		private readonly IEqualityComparer<EventArgs> _equalityComparer = new FileSystemEventArgsEqualityComparer();
		private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim( LockRecursionPolicy.NoRecursion );

		public void AddEvent( [NotNull] EventArgs e )
		{
			if ( e == null )
			{
				throw new ArgumentNullException( "e" );
			}

			string key = GetFileName( e );
			List<EventArgs> list;

			_lock.EnterUpgradeableReadLock();

			if ( !_events.TryGetValue( key, out list ) )
			{
				list = new List<EventArgs>();

				_lock.EnterWriteLock();
				_events.Add( key, list );
				_lock.ExitWriteLock();
			}

			var last = list.LastOrDefault();
			if ( !_equalityComparer.Equals( last, e ) )
			{
				list.Add( e );
			}

			_lock.ExitUpgradeableReadLock();
		}

		public IEnumerable<EventArgs> GetEvents()
		{
			_lock.EnterReadLock();

			try
			{
				return _events.SelectMany( pair => pair.Value ).ToList();
			}
			finally
			{
				_lock.ExitReadLock();
			}
		}

		public void Clear()
		{
			_lock.EnterWriteLock();

			_events.Clear();

			_lock.ExitWriteLock();
		}

		private string GetFileName( EventArgs args )
		{
			if ( args is ErrorEventArgs )
			{
				return String.Empty;
			}
			if ( args is RenamedEventArgs )
			{
				return ((RenamedEventArgs)args).FullPath;
			}
			if ( args is FileSystemEventArgs )
			{
				return ((FileSystemEventArgs)args).FullPath;
			}

			throw new InvalidOperationException( String.Format( "Unexpected EventArgs typs: '{0}'", args.GetType().Name ) );
		}
	}
}