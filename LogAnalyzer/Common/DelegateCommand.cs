using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace LogAnalyzer.GUI.Common
{
	public class DelegateCommand : ICommand
	{
		private readonly Action executeMethod = null;
		private readonly Func<bool> canExecuteMethod = null;
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

		public DelegateCommand( Action executeMethod )
			: this( executeMethod, null, false )
		{
		}

		public DelegateCommand( Action executeMethod, Func<bool> canExecuteMethod )
			: this( executeMethod, canExecuteMethod, false )
		{
		}

		public DelegateCommand( Action executeMethod, Func<bool> canExecuteMethod, bool isAutomaticRequeryDisabled )
		{
			if ( executeMethod == null )
			{
				throw new ArgumentNullException( "executeMethod" );
			}
			this.executeMethod = executeMethod;
			this.canExecuteMethod = canExecuteMethod;
			this.isAutomaticRequeryDisabled = isAutomaticRequeryDisabled;
		}

		[DebuggerStepThrough]
		public bool CanExecute()
		{
			return canExecuteMethod == null || canExecuteMethod();
		}

		[DebuggerStepThrough]
		public void Execute()
		{
#if DEBUG
			if ( !CanExecute() )
			{
				throw new InvalidOperationException( "Command.CanExecute() is false." );
			}
#endif

			if ( executeMethod != null )
			{
				executeMethod();
			}
		}

		public void RaiseCanExecuteChanged()
		{
			this.OnCanExecuteChanged();
		}

		protected virtual void OnCanExecuteChanged()
		{
			CommandManagerHelper.CallWeakReferenceHandlers( canExecuteChangedHandlers );
		}

		[DebuggerStepThrough]
		bool ICommand.CanExecute( object parameter )
		{
			return CanExecute();
		}

		[DebuggerStepThrough]
		void ICommand.Execute( object parameter )
		{
			Execute();
		}
	}
}
