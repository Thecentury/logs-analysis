using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace LogAnalyzer.Kernel
{
	public interface IViewManager<out TViewBase>
	{
		void RegisterView( Type viewType, Type viewModelType );
		TViewBase ResolveView( object viewModel );
	}
}
