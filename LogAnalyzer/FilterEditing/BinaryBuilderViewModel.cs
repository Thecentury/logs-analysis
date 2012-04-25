using LogAnalyzer.Filters;
using System.Linq.Expressions;
using LogAnalyzer.GUI.FilterEditor;

namespace LogAnalyzer.GUI.FilterEditing
{
	internal sealed class BinaryBuilderViewModel : ExpressionBuilderViewModel
	{
		private readonly ExpressionBuilderViewModel _leftViewModel;
		private readonly ExpressionBuilderViewModel _rightViewModel;

		public BinaryBuilderViewModel( BuilderContext<BinaryExpressionBuilder> context )
			: base( context )
		{
			var builder = context.TypedBuilder;

			var leftBuilder = new DelegateBuilderProxy( context.Builder, "Left" );
			var rightBuilder = new DelegateBuilderProxy( context.Builder, "Right" );

			_leftViewModel = ExpressionBuilderViewModelFactory.CreateViewModel( context.WithBuilder( leftBuilder ) );
			if ( builder.Left != null )
			{
				_leftViewModel.SelectedChild = ExpressionBuilderViewModelFactory.CreateViewModel( context.WithBuilder( builder.Left ) );
			}

			_rightViewModel = ExpressionBuilderViewModelFactory.CreateViewModel( context.WithBuilder( rightBuilder ) );
			if ( builder.Right != null )
			{
				_rightViewModel.SelectedChild = ExpressionBuilderViewModelFactory.CreateViewModel( context.WithBuilder( builder.Right ) );
			}
		}

		public ExpressionBuilderViewModel Left
		{
			get { return _leftViewModel; }
		}

		public ExpressionBuilderViewModel Right
		{
			get { return _rightViewModel; }
		}
	}
}
