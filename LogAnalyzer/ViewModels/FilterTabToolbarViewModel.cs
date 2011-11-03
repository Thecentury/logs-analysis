using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace LogAnalyzer.GUI.ViewModels
{
	internal sealed class FilterTabToolbarViewModel : BindingObject
	{
		private readonly FilterTabViewModel tabViewModel;

		public FilterTabToolbarViewModel( FilterTabViewModel tabViewModel )
		{
			this.tabViewModel = tabViewModel;
		}

		public ICommand EditFilterCommand
		{
			get { return tabViewModel.EditFilterCommand; }
		}

		public ICommand RefreshCommand
		{
			get { return tabViewModel.RefreshCommand; }
		}
	}
}