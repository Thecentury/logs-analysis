using System;
using System.Collections.Generic;

namespace LogAnalyzer.Kernel.Notifications
{
	public sealed class CompositeLogNotificationsSource : SubscribableLogNotificationSourceBase
	{
		private readonly List<LogNotificationsSourceBase> _children = new List<LogNotificationsSourceBase>();

		public CompositeLogNotificationsSource( params LogNotificationsSourceBase[] children )
			: this( (IEnumerable<LogNotificationsSourceBase>)children ) { }

		public CompositeLogNotificationsSource( IEnumerable<LogNotificationsSourceBase> children )
		{
			if ( children == null ) throw new ArgumentNullException( "children" );

			this._children.AddRange( children );

			foreach ( var child in this._children )
			{
				SubscribeToEvents( child );
			}
		}

		protected override void StartCore()
		{
			base.StartCore();

			foreach ( var child in _children )
			{
				child.Start();
			}
		}

		protected override void StopCore()
		{
			foreach ( var child in _children )
			{
				child.Stop();
			}

			base.StopCore();
		}
	}
}
