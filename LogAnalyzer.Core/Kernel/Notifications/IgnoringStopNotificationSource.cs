using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace LogAnalyzer.Kernel.Notifications
{
	public sealed class IgnoringStopNotificationSource : LogNotificationsSourceBase
	{
		private readonly LogNotificationsSourceBase _inner;

		public IgnoringStopNotificationSource( [NotNull] LogNotificationsSourceBase inner )
		{
			if ( inner == null )
			{
				throw new ArgumentNullException( "inner" );
			}
			_inner = inner;

			inner.Created += InnerCreated;
			inner.Changed += InnerChanged;
			inner.Deleted += InnerDeleted;
			inner.Renamed += InnerRenamed;
			inner.Error += InnerError;
		}

		private void InnerError( object sender, ErrorEventArgs e )
		{
			RaiseError( e );
		}

		private void InnerRenamed( object sender, RenamedEventArgs e )
		{
			RaiseRenamed( e );
		}

		private void InnerDeleted( object sender, FileSystemEventArgs e )
		{
			RaiseDeleted( e );
		}

		private void InnerChanged( object sender, FileSystemEventArgs e )
		{
			RaiseChanged( e );
		}

		private void InnerCreated( object sender, FileSystemEventArgs e )
		{
			RaiseCreated( e );
		}

		protected override void StartCore()
		{
			base.StartCore();
			_inner.Start();
		}

		protected override void StopCore()
		{
			base.StopCore();
		}

		protected override IEnumerable<LogNotificationsSourceBase> GetChildren()
		{
			yield return _inner;
		}
	}
}
