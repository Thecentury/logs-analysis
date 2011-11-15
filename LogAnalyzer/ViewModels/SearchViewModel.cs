using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using LogAnalyzer.GUI.Common;

namespace LogAnalyzer.GUI.ViewModels
{
	public sealed class SearchViewModel : BindingObject
	{
		private DelegateCommand findNextCommand;
		public ICommand FindNextCommand
		{
			get { return findNextCommand; }
		}

		private DelegateCommand findPreviousCommand;
		public ICommand FindPreviousCommand
		{
			get { return findPreviousCommand; }
		}
	}
}
