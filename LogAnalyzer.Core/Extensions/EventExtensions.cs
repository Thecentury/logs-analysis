using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;

namespace LogAnalyzer.Extensions
{
	public static class EventExtensions
	{
		public static void RaiseCollectionReset( this NotifyCollectionChangedEventHandler @event, object sender )
		{
			if ( @event != null )
			{
				@event( sender, new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
			}
		}

		public static void RaiseCollectionChanged( this NotifyCollectionChangedEventHandler @event, object sender, NotifyCollectionChangedEventArgs args )
		{
			if ( @event != null )
			{
				@event( sender, args );
			}
		}

		public static void Raise( this PropertyChangedEventHandler @event, object sender, string propertyName )
		{
#if DEBUG
			bool containsProperty = String.IsNullOrEmpty( propertyName ) || sender.GetType()
				.GetProperties( BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance )
				.Any( p => p.Name == propertyName );

			if ( !containsProperty )
			{
				bool ignore = sender.GetType()
					.GetCustomAttributes( typeof( IgnoreMissingPropertyAttribute ), true )
					.Cast<IgnoreMissingPropertyAttribute>().Any( attr => attr.PropertyName == propertyName );

				if ( !ignore )
				{
					Condition.DebugAssert( containsProperty );
				}
			}
#endif

			if ( @event != null )
			{
				@event( sender, new PropertyChangedEventArgs( propertyName ) );
			}
		}

		public static void RaiseAllChanged( this PropertyChangedEventHandler @event, object sender )
		{
			if ( @event != null )
			{
				@event( sender, new PropertyChangedEventArgs( "" ) );
			}
		}

		public static void Raise( this EventHandler @event, object sender )
		{
			if ( @event != null )
			{
				@event( sender, EventArgs.Empty );
			}
		}

		public static void Raise<T>( this EventHandler<T> @event, object sender, T eventArgs )
			where T : EventArgs
		{
			if ( @event != null )
			{
				@event( sender, eventArgs );
			}
		}
	}
}
