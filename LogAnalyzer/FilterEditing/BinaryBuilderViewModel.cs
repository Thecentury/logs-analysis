using ExpressionBuilderSample;
using LogAnalyzer.Filters;
using System.Linq.Expressions;
using LogAnalyzer.GUI.FilterEditor;

namespace LogAnalyzer.GUI.FilterEditing
{
	internal class BinaryBuilderViewModel : ExpressionBuilderViewModel
	{
		private readonly ExpressionBuilderViewModel leftViewModel;
		private readonly ExpressionBuilderViewModel rightViewModel;

		public BinaryBuilderViewModel( BinaryExpressionBuilder builder, ParameterExpression parameter )
			: base( builder, parameter )
		{
			var leftBuilder = builder.Left ?? new DelegateBuilderProxy( builder, "Left" );
			var rightBuilder = builder.Right ?? new DelegateBuilderProxy( builder, "Right" );

			leftViewModel = ExpressionBuilderViewModelFactory.CreateViewModel( leftBuilder, parameter );
			rightViewModel = ExpressionBuilderViewModelFactory.CreateViewModel( rightBuilder, parameter );
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
