using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdTech.Common.WPF;
using System.Windows.Input;
using LogAnalyzer.Filters;
using System.Windows;
using LogAnalyzer.GUI.View;

namespace LogAnalyzer.GUI.ViewModel
{
	internal sealed class FilterEditorViewModel : BindingObject
	{
		private readonly FilterEditorWindow window = null;

		public FilterEditorViewModel( FilterEditorWindow window )
		{
			if ( window == null )
				throw new ArgumentNullException( "window" );

			this.window = window;
			window.DataContext = this;
		}

		private DelegateCommand okCommand = null;
		public ICommand OkCommand
		{
			get
			{
				if ( okCommand == null )
				{
					okCommand = new DelegateCommand( OkExecute, CanOkExecute );
				}
				return okCommand;
			}
		}

		private void OkExecute()
		{
			window.DialogResult = true;
			window.Close();
		}

		private bool CanOkExecute()
		{
			return window.Builder != null;
		}

		private DelegateCommand closeCommand = null;
		public ICommand CloseCommand
		{
			get
			{
				if ( closeCommand == null )
				{
					closeCommand = new DelegateCommand( CloseExecute );
				}

				return closeCommand;
			}
		}

		public void CloseExecute()
		{
			window.DialogResult = false;
			window.Close();
		}

		public ExpressionBuilder Builder
		{
			get { return window.Builder; }
		}
	}
}
