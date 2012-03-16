using System;
using System.ComponentModel;
using LogAnalyzer.Extensions;
using System.Windows;
using System.Windows.Threading;
using System.Diagnostics;

namespace LogAnalyzer.GUI.ViewModels
{
	/// <summary>
	/// Обеспечивает уведомление об изменении значений свойств.
	/// </summary>
	public abstract class BindingObject : INotifyPropertyChanged, IDisposable, IFreezable
	{
		private readonly INotifyPropertyChanged observableObject;
		// todo probably use 'observableObject' as a lock
		private readonly object sync = new object();

		protected BindingObject() { }

		protected BindingObject( object inner )
		{
			if ( inner == null )
				throw new ArgumentNullException( "inner" );

			IFreezable freezable = inner as IFreezable;
			bool frozen = freezable != null && freezable.IsFrozen;

			observableObject = inner as INotifyPropertyChanged;
			if ( observableObject != null && !frozen )
			{
				observableObject.PropertyChanged += OnInnerPropertyChanged;
			}
		}

		protected virtual void OnInnerPropertyChanged( object sender, PropertyChangedEventArgs e )
		{
			propertyChanged.Raise( this, e.PropertyName );
		}

		[DebuggerStepThrough]
		protected void InvokeInUIDispatcher( Action action, DispatcherPriority priority = DispatcherPriority.Normal )
		{
			Dispatcher dispatcher = DispatcherHelper.GetDispatcher();
			dispatcher.Invoke( action, priority );
		}

		protected void BeginInvokeInUIDispatcher( Action action, DispatcherPriority priority = DispatcherPriority.Normal)
		{
			Dispatcher dispatcher = DispatcherHelper.GetDispatcher();
			dispatcher.BeginInvoke( action, priority );
		}

		protected virtual void OnPropertyChangedSubscribe() { }
		protected virtual void OnPropertyChangedUnsubscribe() { }

		#region INotifyPropertyChanged Members

		// todo implement add-remove
		private PropertyChangedEventHandler propertyChanged;

		public event PropertyChangedEventHandler PropertyChanged
		{
			add
			{
				OnPropertyChangedSubscribe();

				// не подписываем на события, если Frozen
				if ( isFrozen )
				{
					//Debug.WriteLine(String.Format("{0}: somebody tried to subscribe to PropertyChanged of frozen object.", this.GetType().Name));
					return;
				}

				lock ( sync )
				{
					propertyChanged = (PropertyChangedEventHandler)Delegate.Combine( propertyChanged, value );
				}
			}
			remove
			{
				OnPropertyChangedUnsubscribe();

				if ( isFrozen )
				{
					return;
				}

				lock ( sync )
				{
					propertyChanged = (PropertyChangedEventHandler)Delegate.Remove( propertyChanged, value );
				}
			}
		}

		protected void RaiseAllPropertiesChanged()
		{
			propertyChanged.Raise( this, String.Empty );
		}

		protected void RaisePropertyChanged( string propertyName )
		{
			propertyChanged.Raise( this, propertyName );
		}

		protected void RaisePropertiesChanged( params string[] propertiesNames )
		{
			if ( propertiesNames == null )
				throw new ArgumentNullException( "propertiesNames" );

			foreach ( string propertyName in propertiesNames )
			{
				propertyChanged.Raise( this, propertyName );
			}
		}

		#endregion

		#region IDisposable Members

		public virtual void Dispose()
		{
			// отписка от событий вложенного объекта
			if ( observableObject != null )
			{
				observableObject.PropertyChanged -= OnInnerPropertyChanged;
			}
		}

		#endregion

		#region IFreezable Members

		public virtual void Freeze()
		{
			// already frozen?
			if ( isFrozen )
				throw new InvalidOperationException();

			isFrozen = true;
			propertyChanged = null;
		}

		private bool isFrozen = false;
		public bool IsFrozen
		{
			get { return isFrozen; }
		}

		#endregion
	}
}
