using LogAnalyzer.Filters;
using System.Linq.Expressions;
using LogAnalyzer.GUI.FilterEditor;

namespace LogAnalyzer.GUI.FilterEditing
{
	internal sealed class BinaryBuilderViewModel : ExpressionBuilderViewModel
	{
		private readonly ExpressionBuilderViewModel _leftViewModel;
		private readonly ExpressionBuilderViewModel _rightViewModel;

		public BinaryBuilderViewModel( BinaryExpressionBuilder builder, ParameterExpression parameter )
			: base( builder, parameter )
		{
			var leftBuilder = new DelegateBuilderProxy( builder, "Left" );
			var rightBuilder = new DelegateBuilderProxy( builder, "Right" );

			_leftViewModel = ExpressionBuilderViewModelFactory.CreateViewModel( leftBuilder, parameter );
			if ( builder.Left != null )
			{
				_leftViewModel.SelectedChild = ExpressionBuilderViewModelFactory.CreateViewModel( builder.Left, parameter );
			}

			_rightViewModel = ExpressionBuilderViewModelFactory.CreateViewModel( rightBuilder, parameter );
			if ( builder.Right != null )
			{
				_rightViewModel.SelectedChild = ExpressionBuilderViewModelFactory.CreateViewModel( builder.Right, parameter );
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
