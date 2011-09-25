using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdTech.Common.WPF;
using System.Windows.Input;
using System.Windows;

namespace LogAnalyzer.GUI.ViewModel
{
	public class TabViewModel : BindingObject
	{
		private readonly ApplicationViewModel applicationViewModel = null;
		public ApplicationViewModel ApplicationViewModel
		{
			get { return applicationViewModel; }
		}

		protected TabViewModel( ApplicationViewModel applicationViewModel )
		{
			if ( applicationViewModel == null )
				throw new ArgumentNullException( "applicationViewModel" );

			this.applicationViewModel = applicationViewModel;
		}

		public string Type
		{
			get { return GetType().Name; }
		}

		public virtual string Header { get { return "<HEADER NOT SET>"; } }
		public virtual bool IsActive { get; set; }
		public virtual string IconFile
		{
			get { return null; }
		}

		public bool CanBeClosed
		{
			get { return CanBeClosedCore(); }
		}

		protected virtual bool CanBeClosedCore()
		{
			return true;
		}

		public void Close()
		{
			if ( CanBeClosedCore() )
			{
				OnClosing();
				applicationViewModel.Tabs.Remove( this );
			}
		}

		protected virtual void OnClosing() { }

		private DelegateCommand closeCommand = null;
		public ICommand CloseCommand
		{
			get
			{
				if ( closeCommand == null )
				{
					closeCommand = new DelegateCommand( Close, CanBeClosedCore );
				}

				return closeCommand;
			}
		}

		public Visibility CloseButtonVisibility
		{
			get { return CanBeClosed ? Visibility.Visible : Visibility.Collapsed; }
		}

		// todo Clone and Close and CloseAll and others similar commands
	}
}
