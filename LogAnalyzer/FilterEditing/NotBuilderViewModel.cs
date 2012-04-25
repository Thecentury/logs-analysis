using LogAnalyzer.Filters;
using System.Linq.Expressions;

namespace LogAnalyzer.GUI.FilterEditing
{
	internal sealed class NotBuilderViewModel : ExpressionBuilderViewModel
	{
		private readonly Not _notBuilder;
		private readonly ExpressionBuilderViewModel _innerViewModel;

		public NotBuilderViewModel( BuilderContext<Not> ctx )
			: base( ctx )
		{
			_notBuilder = ctx.TypedBuilder;
			_innerViewModel = ExpressionBuilderViewModelFactory.CreateViewModel( ctx.WithBuilder( new DelegateBuilderProxy( _notBuilder, "Inner" ) ) );

			if ( _notBuilder.Inner != null )
			{
				_innerViewModel.SelectedChild = ExpressionBuilderViewModelFactory.CreateViewModel( ctx.WithBuilder( _notBuilder.Inner ) );
			}
		}

		public ExpressionBuilderViewModel Inner
		{
			get { return _innerViewModel; }
		}

		protected override void OnSelectedChildChanged( ExpressionBuilder builder )
		{
			_notBuilder.Inner = builder;
		}
	}
}
