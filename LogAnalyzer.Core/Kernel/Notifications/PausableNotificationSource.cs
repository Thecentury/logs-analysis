using System;
using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace LogAnalyzer.Kernel.Notifications
{
	public sealed class PausableNotificationSource : LogNotificationsSourceBase
	{
		private readonly LogNotificationsSourceBase _inner;
		private readonly ChangesStorage _changesStorage = new ChangesStorage();

		public PausableNotificationSource( [NotNull] LogNotificationsSourceBase inner )
		{
			if ( inner == null )
			{
				throw new ArgumentNullException( "inner" );
			}
			_inner = inner;
			_inner.Changed += OnInnerChanged;
			_inner.Created += OnInnerCreated;
			_inner.Deleted += OnInnerDeleted;
			_inner.Error += OnInnerError;
			_inner.Renamed += OnInnerRenamed;
		}

		protected override void StartCore()
		{
			base.StartCore();

			var events = _changesStorage.GetEvents();

			foreach ( var evt in events )
			{
				Raise( evt );
			}

			_changesStorage.Clear();

			_inner.Start();
		}

		protected override void StopCore()
		{
			base.StopCore();
			_inner.Stop();
		}

		private void OnInnerRenamed( object sender, RenamedEventArgs e )
		{
			if ( IsEnabled )
			{
				RaiseRenamed( e );
			}
			else
			{
				_changesStorage.AddEvent( e );
			}
		}

		private void OnInnerError( object sender, ErrorEventArgs e )
		{
			if ( IsEnabled )
			{
				RaiseError( e );
			}
			else
			{
				_changesStorage.AddEvent( e );
			}
		}

		private void OnInnerDeleted( object sender, FileSystemEventArgs e )
		{
			if ( IsEnabled )
			{
				RaiseDeleted( e );
			}
			else
			{
				_changesStorage.AddEvent( e );
			}
		}

		private void OnInnerCreated( object sender, FileSystemEventArgs e )
		{
			if ( IsEnabled )
			{
				RaiseCreated( e );
			}
			else
			{
				_changesStorage.AddEvent( e );
			}
		}

		private void OnInnerChanged( object sender, FileSystemEventArgs e )
		{
			if ( IsEnabled )
			{
				RaiseChanged( e );
			}
			else
			{
				_changesStorage.AddEvent( e );
			}
		}
	}
}
