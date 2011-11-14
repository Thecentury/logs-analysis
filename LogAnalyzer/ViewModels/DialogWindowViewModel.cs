using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using JetBrains.Annotations;

namespace LogAnalyzer.GUI.ViewModels
{
	public abstract class DialogWindowViewModel : DialogViewModel
	{
		private readonly Window window;

		protected DialogWindowViewModel( [NotNull] Window window )
		{
			if ( window == null ) throw new ArgumentNullException( "window" );
			this.window = window;
			window.DataContext = this;
		}

		public Window Window
		{
			get { return window; }
		}

		protected override void OkExecute()
		{
			base.OkExecute();
			Window.Close();
		}

		protected override void CloseExecute()
		{
			base.CloseExecute();
			window.Close();
		}
	}
}
