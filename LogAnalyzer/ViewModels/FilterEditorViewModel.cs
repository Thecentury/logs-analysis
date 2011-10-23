using System;
using AdTech.Common.WPF;
using System.Windows.Input;
using LogAnalyzer.Filters;
using LogAnalyzer.GUI.Views;

namespace LogAnalyzer.GUI.ViewModels
{
	internal sealed class FilterEditorViewModel : BindingObject
	{
		private readonly FilterEditorWindow window;

		public FilterEditorViewModel( FilterEditorWindow window )
		{
			if ( window == null )
				throw new ArgumentNullException( "window" );

			this.window = window;
			window.DataContext = this;
		}

		private DelegateCommand okCommand;
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
			var builder = window.Builder;

			bool isBuilderFull = false;
			if ( builder != null )
			{
				isBuilderFull = builder.ValidateProperties();
			}
			
			return isBuilderFull;
		}

		private DelegateCommand closeCommand;
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
			set { window.Builder = value; }
		}
	}
}
