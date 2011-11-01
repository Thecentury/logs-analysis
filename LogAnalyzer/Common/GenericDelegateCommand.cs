using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace LogAnalyzer.GUI.Common
{
	public class DelegateCommand<T> : ICommand
	{
		private readonly Action<T> executeMethod = null;
		private readonly Func<T, bool> canExecuteMethod = null;
		private bool isAutomaticRequeryDisabled = false;
		private List<WeakReference> canExecuteChangedHandlers;
		public event EventHandler CanExecuteChanged
		{
			add
			{
				if ( !this.isAutomaticRequeryDisabled )
				{
					CommandManager.RequerySuggested += value;
				}
				CommandManagerHelper.AddWeakReferenceHandler( ref this.canExecuteChangedHandlers, value, 2 );
			}
			remove
			{
				if ( !this.isAutomaticRequeryDisabled )
				{
					CommandManager.RequerySuggested -= value;
				}
				CommandManagerHelper.RemoveWeakReferenceHandler( this.canExecuteChangedHandlers, value );
			}
		}
		public bool IsAutomaticRequeryDisabled
		{
			get
			{
				return this.isAutomaticRequeryDisabled;
			}
			set
			{
				if ( this.isAutomaticRequeryDisabled != value )
				{
					if ( value )
					{
						CommandManagerHelper.RemoveHandlersFromRequerySuggested( this.canExecuteChangedHandlers );
					}
					else
					{
						CommandManagerHelper.AddHandlersToRequerySuggested( this.canExecuteChangedHandlers );
					}
					this.isAutomaticRequeryDisabled = value;
				}
			}
		}
		public DelegateCommand( Action<T> executeMethod )
			: this( executeMethod, null, false )
		{
		}
		public DelegateCommand( Action<T> executeMethod, Func<T, bool> canExecuteMethod )
			: this( executeMethod, canExecuteMethod, false )
		{
		}
		public DelegateCommand( Action<T> executeMethod, Func<T, bool> canExecuteMethod, bool isAutomaticRequeryDisabled )
		{
			if ( executeMethod == null )
			{
				throw new ArgumentNullException( "executeMethod" );
			}
			this.executeMethod = executeMethod;
			this.canExecuteMethod = canExecuteMethod;
			this.isAutomaticRequeryDisabled = isAutomaticRequeryDisabled;
		}
		public bool CanExecute( T parameter )
		{
			return this.canExecuteMethod == null || this.canExecuteMethod( parameter );
		}
		public void Execute( T parameter )
		{
			if ( this.executeMethod != null )
			{
				this.executeMethod( parameter );
			}
		}
		public void RaiseCanExecuteChanged()
		{
			this.OnCanExecuteChanged();
		}
		protected virtual void OnCanExecuteChanged()
		{
			CommandManagerHelper.CallWeakReferenceHandlers( this.canExecuteChangedHandlers );
		}
		private T TryCast( object parameter )
		{
			T result;
			if ( parameter == null )
			{
				result = default( T );
			}
			else
			{
				if ( !typeof( T ).IsAssignableFrom( parameter.GetType() ) )
				{
					throw new ArgumentException( string.Format( "Wrong command parameter type inferred. Cannot convert type {0} to the preferred parameter type {1}", parameter.GetType(), typeof( T ) ) );
				}
				result = (T)parameter;
			}
			return result;
		}
		bool ICommand.CanExecute( object parameter )
		{
			bool result;
			if ( parameter == null && typeof( T ).IsValueType )
			{
				result = (this.canExecuteMethod == null);
			}
			else
			{
				result = this.CanExecute( this.TryCast( parameter ) );
			}
			return result;
		}
		void ICommand.Execute( object parameter )
		{
			this.Execute( this.TryCast( parameter ) );
		}
	}
}
