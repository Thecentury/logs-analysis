using System.Linq.Expressions;
using LogAnalyzer.Filters;
using LogAnalyzer.GUI.FilterEditor;

namespace LogAnalyzer.GUI.FilterEditing
{
	internal sealed class GetPropertyBuilderViewModel : ExpressionBuilderViewModel
	{
		private readonly ExpressionBuilderViewModel _targetViewModel;
		private readonly GetProperty _getPropertyBuilder;

		public GetPropertyBuilderViewModel( BuilderContext<GetProperty> ctx )
			: base( ctx )
		{
			_getPropertyBuilder = ctx.TypedBuilder;
			_targetViewModel = ExpressionBuilderViewModelFactory.CreateViewModel( ctx.WithBuilder( new DelegateBuilderProxy( _getPropertyBuilder, "Target" ) ) );
			if ( _getPropertyBuilder.Target != null )
			{
				_targetViewModel.SelectedChild = ExpressionBuilderViewModelFactory.CreateViewModel( ctx.WithBuilder( _getPropertyBuilder.Target) );
			}
		}

		public ExpressionBuilderViewModel Target
		{
			get { return _targetViewModel; }
		}

		protected override void OnSelectedChildChanged( ExpressionBuilder builder )
		{
			_getPropertyBuilder.Target = builder;
		}
	}
}
