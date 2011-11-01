using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace LogAnalyzer.GUI.Common
{
	internal static class CommandManagerHelper
	{
		internal static void CallWeakReferenceHandlers( List<WeakReference> handlers )
		{
			if ( handlers != null )
			{
				EventHandler[] array = new EventHandler[handlers.Count];
				int num = 0;
				for ( int i = handlers.Count - 1; i >= 0; i-- )
				{
					WeakReference weakReference = handlers[i];
					EventHandler eventHandler = weakReference.Target as EventHandler;
					if ( eventHandler == null )
					{
						handlers.RemoveAt( i );
					}
					else
					{
						array[num] = eventHandler;
						num++;
					}
				}
				for ( int i = 0; i < num; i++ )
				{
					EventHandler eventHandler = array[i];
					eventHandler( null, EventArgs.Empty );
				}
			}
		}
		internal static void AddHandlersToRequerySuggested( List<WeakReference> handlers )
		{
			if ( handlers != null )
			{
				foreach ( WeakReference current in handlers )
				{
					EventHandler eventHandler = current.Target as EventHandler;
					if ( eventHandler != null )
					{
						CommandManager.RequerySuggested += eventHandler;
					}
				}
			}
		}
		internal static void RemoveHandlersFromRequerySuggested( List<WeakReference> handlers )
		{
			if ( handlers != null )
			{
				foreach ( WeakReference current in handlers )
				{
					EventHandler eventHandler = current.Target as EventHandler;
					if ( eventHandler != null )
					{
						CommandManager.RequerySuggested -= eventHandler;
					}
				}
			}
		}
		internal static void AddWeakReferenceHandler( ref List<WeakReference> handlers, EventHandler handler )
		{
			CommandManagerHelper.AddWeakReferenceHandler( ref handlers, handler, -1 );
		}
		internal static void AddWeakReferenceHandler( ref List<WeakReference> handlers, EventHandler handler, int defaultListSize )
		{
			if ( handlers == null )
			{
				handlers = ((defaultListSize > 0) ? new List<WeakReference>( defaultListSize ) : new List<WeakReference>());
			}
			handlers.Add( new WeakReference( handler ) );
		}
		internal static void RemoveWeakReferenceHandler( List<WeakReference> handlers, EventHandler handler )
		{
			if ( handlers != null )
			{
				for ( int num = handlers.Count - 1; num >= 0; num-- )
				{
					WeakReference weakReference = handlers[num];
					EventHandler eventHandler = weakReference.Target as EventHandler;
					if ( eventHandler == null || eventHandler == handler )
					{
						handlers.RemoveAt( num );
					}
				}
			}
		}
	}
}
