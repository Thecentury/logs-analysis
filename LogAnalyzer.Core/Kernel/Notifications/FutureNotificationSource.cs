using System;
using JetBrains.Annotations;

namespace LogAnalyzer.Kernel.Notifications
{
	public sealed class FutureNotificationSource : SubscribableLogNotificationSourceBase
	{
		private readonly Func<LogNotificationsSourceBase> _createChildHandler;
		private LogNotificationsSourceBase _child;

		public FutureNotificationSource([NotNull] Func<LogNotificationsSourceBase> createChildHandler)
		{
			if (createChildHandler == null)
			{
				throw new ArgumentNullException("createChildHandler");
			}
			_createChildHandler = createChildHandler;
		}

		private void EnsureChildExists()
		{
			if ( _child == null )
			{
				_child = _createChildHandler();
				SubscribeToEvents( _child );
			}
		}

		protected override void StartCore()
		{
			EnsureChildExists();
			_child.Start();

			base.StartCore();
		}

		protected override void StopCore()
		{
			EnsureChildExists();
			_child.Stop();

			base.StopCore();
		}
	}
}