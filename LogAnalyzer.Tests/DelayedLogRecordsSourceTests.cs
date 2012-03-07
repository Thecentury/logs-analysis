using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Extensions;
using System.IO;
using System.Reactive.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using LogAnalyzer.Kernel;
using LogAnalyzer.Kernel.Notifications;
using LogAnalyzer.Tests.Mocks;
using NUnit.Framework;

namespace LogAnalyzer.Tests
{
	[TestFixture]
	public class DelayedLogRecordsSourceTests
	{
		/// <summary>
		/// Проверка того, что DelayedLogRecordsSource работает правильно, объединяя уведомление о последовательных во времени записях.
		/// </summary>
		[TestCase( 10 )]
		[TestCase( 1000 )]
		[Test]
		public void TestDelayedLogRecordsSource( int notificationDelay )
		{
			MockLogRecordsSource mockSource = new MockLogRecordsSource( "Dir" );
			DelayedLogRecordsSource delayedSource = new DelayedLogRecordsSource( mockSource, TimeSpan.FromMilliseconds( notificationDelay ) );
			delayedSource.Start();

			ConcurrentDictionary<string, int> calledTimes = new ConcurrentDictionary<string, int>();
			calledTimes["1"] = 0;
			calledTimes["2"] = 0;
			calledTimes["3"] = 0;

			var changedEvents = Observable.FromEventPattern<FileSystemEventArgs>( delayedSource, "Changed" )
				.Select( e => e.EventArgs.Name )
				.Do( name => Debug.WriteLine( "Called " + name ) )
				.Subscribe( name => calledTimes[name] = calledTimes[name] + 1 );

			using ( Task task = Task.Factory.StartNew( () =>
			{
				mockSource.RaiseFileChanged( "1" );
				mockSource.RaiseFileChanged( "1" );

				mockSource.RaiseFileChanged( "2" );
				mockSource.RaiseFileChanged( "2" );

				mockSource.RaiseFileChanged( "3" );
				mockSource.RaiseFileChanged( "3" );

				Thread.Sleep( notificationDelay * 2 + 100 );

				mockSource.RaiseFileChanged( "1" );
				mockSource.RaiseFileChanged( "1" );
			} ) )
			{
				task.Wait();
			}

			Thread.Sleep( notificationDelay * 4 );

			Assert.AreEqual( 2, calledTimes["1"] );
			Assert.AreEqual( 1, calledTimes["2"] );
			Assert.AreEqual( 1, calledTimes["3"] );
		}
	}
}
