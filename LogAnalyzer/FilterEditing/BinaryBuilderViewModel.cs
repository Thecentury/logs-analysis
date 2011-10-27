using LogAnalyzer.Filters;
using System.Linq.Expressions;
using LogAnalyzer.GUI.FilterEditor;

namespace LogAnalyzer.GUI.FilterEditing
{
	internal sealed class BinaryBuilderViewModel : ExpressionBuilderViewModel
	{
		private readonly ExpressionBuilderViewModel leftViewModel;
		private readonly ExpressionBuilderViewModel rightViewModel;

		public BinaryBuilderViewModel( BinaryExpressionBuilder builder, ParameterExpression parameter )
			: base( builder, parameter )
		{
			var leftBuilder = new DelegateBuilderProxy( builder, "Left" );
			var rightBuilder = new DelegateBuilderProxy( builder, "Right" );

			leftViewModel = ExpressionBuilderViewModelFactory.CreateViewModel( leftBuilder, parameter );
			if ( builder.Left != null )
			{
				leftViewModel.SelectedChild = ExpressionBuilderViewModelFactory.CreateViewModel( builder.Left, parameter );
			}

			rightViewModel = ExpressionBuilderViewModelFactory.CreateViewModel( rightBuilder, parameter );
			if ( builder.Right != null )
			{
				rightViewModel.SelectedChild = ExpressionBuilderViewModelFactory.CreateViewModel( builder.Right, parameter );
			}
		}

		public ExpressionBuilderViewModel Left
		{
			get { return leftViewModel; }
		}

		public ExpressionBuilderViewModel Right
		{
			get { return rightViewModel; }
		}
	}
}
