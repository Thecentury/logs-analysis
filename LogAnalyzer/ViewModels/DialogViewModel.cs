using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using LogAnalyzer.GUI.Common;

namespace LogAnalyzer.GUI.ViewModels
{
	public abstract class DialogViewModel : BindingObject
	{
		private DelegateCommand okCommand;
		public ICommand OkCommand
		{
			get
			{
				if ( okCommand == null )
					okCommand = new DelegateCommand( OkExecute, CanOkExecute );

				return okCommand;
			}
		}

		protected virtual void OkExecute()
		{
			DialogResult = true;
		}

		protected virtual bool CanOkExecute()
		{
			return true;
		}

		private DelegateCommand closeCommand;
		public ICommand CloseCommand
		{
			get
			{
				if ( closeCommand == null )
					closeCommand = new DelegateCommand( CloseExecute );

				return closeCommand;
			}
		}

		protected virtual void CloseExecute()
		{
			DialogResult = false;
		}

		private bool dialogResult;
		public bool DialogResult
		{
			get { return dialogResult; }
			private set
			{
				dialogResult = value;
				RaisePropertyChanged( "DialogResult" );
			}
		}
	}
}
