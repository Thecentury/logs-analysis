using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using LogAnalyzer.Extensions;
using LogAnalyzer.Kernel;
using Expression = System.Linq.Expressions.Expression;

namespace ModuleLogsProvider.Logging.Most
{
	public sealed class WpfViewManager : IViewManager<FrameworkElement>
	{
		private readonly Dictionary<Type, Func<FrameworkElement>> viewsMappings =
			new Dictionary<Type, Func<FrameworkElement>>();

		public void RegisterView( Type viewType, Type viewModelType )
		{
			if ( viewType == null ) throw new ArgumentNullException( "viewType" );
			if ( viewModelType == null ) throw new ArgumentNullException( "viewModelType" );

			var createViewInstanceFunc = Expression.Lambda<Func<FrameworkElement>>( Expression.New( viewType ) ).Compile();

			viewsMappings.Add( viewModelType, createViewInstanceFunc );
		}

		public FrameworkElement ResolveView( object viewModel )
		{
			if ( viewModel == null ) throw new ArgumentNullException( "viewModel" );

			var createViewFunc = viewsMappings[viewModel.GetType()];
			Dispatcher dispatcher = DispatcherHelper.GetDispatcher();
			FrameworkElement view = null;

			dispatcher.Invoke( () =>
			{
				view = createViewFunc();
				view.DataContext = viewModel;
			}, DispatcherPriority.Send );

			return view;
		}
	}
}
