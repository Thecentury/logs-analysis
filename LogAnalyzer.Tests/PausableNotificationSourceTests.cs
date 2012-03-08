using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using LogAnalyzer.Kernel.Notifications;
using LogAnalyzer.Tests.Mocks;
using NUnit.Framework;

namespace LogAnalyzer.Tests
{
	[TestFixture]
	public class PausableNotificationSourceTests
	{
		private List<EventArgs> _events;
		private PausableNotificationSource _pausable;
		private IgnoringStopNotificationSource _nonStoppable;
		private readonly MockLogRecordsSource _mockLogRecordsSource = new MockLogRecordsSource( "dir" );
		private IDisposable _s1;
		private IDisposable _s2;
		private IDisposable _s3;
		private IDisposable _s4;
		private IDisposable _s5;

		[SetUp]
		public void Setup()
		{
			_nonStoppable = new IgnoringStopNotificationSource( _mockLogRecordsSource );
			_nonStoppable.Start();
			_pausable = new PausableNotificationSource( _nonStoppable );
			_events = new List<EventArgs>();

			_s1 = Observable.FromEventPattern<RenamedEventHandler, RenamedEventArgs>(
				h => _pausable.Renamed += h,
				h => _pausable.Renamed -= h )
				.Subscribe( AddToEvents );

			_s2 = Observable.FromEventPattern<ErrorEventHandler, ErrorEventArgs>(
				h => _pausable.Error += h,
				h => _pausable.Error -= h )
				.Subscribe( AddToEvents );

			_s3 = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
				h => _pausable.Created += h,
				h => _pausable.Created -= h )
				.Subscribe( AddToEvents );

			_s4 = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
				h => _pausable.Changed += h,
				h => _pausable.Changed -= h )
				.Subscribe( AddToEvents );

			_s5 = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
				h => _pausable.Deleted += h,
				h => _pausable.Deleted -= h )
				.Subscribe( AddToEvents );
		}

		[TearDown]
		public void TearDown()
		{
			_s1.Dispose();
			_s2.Dispose();
			_s3.Dispose();
			_s4.Dispose();
			_s5.Dispose();
		}

		private void AddToEvents<T>( EventPattern<T> evt ) where T : EventArgs
		{
			_events.Add( evt.EventArgs );
		}

		[Test]
		public void ShouldRaiseChangedIfEnabled()
		{
			_pausable.Start();
			_mockLogRecordsSource.RaiseFileChanged( "f" );

			Assert.That( _events.Count, Is.EqualTo( 1 ) );
		}

		[Test]
		public void ShouldRaiseChangedWhenEnabled()
		{
			_mockLogRecordsSource.RaiseFileChanged( "f" );

			Assert.That( _events.Count, Is.EqualTo( 0 ) );

			_pausable.Start();

			Assert.That( _events.Count, Is.EqualTo( 1 ) );
		}

		[Test]
		public void ShouldRaiseTwoChangedForTwoFiles()
		{
			_mockLogRecordsSource.RaiseFileChanged( "f1" );
			_mockLogRecordsSource.RaiseFileChanged( "f2" );

			Assert.That( _events, Is.Empty );

			_pausable.Start();

			Assert.That( _events, Has.Count.EqualTo( 2 ) );
		}

		[Test]
		public void ShouldRaiseChangedAndCreated()
		{
			_mockLogRecordsSource.RaiseFileChanged( "f" );
			_mockLogRecordsSource.RaiseFileCreated( "f" );

			Assert.That( _events, Is.Empty );

			_pausable.Start();

			Assert.That( _events, Has.Count.EqualTo( 2 ) );
		}
	}
}
