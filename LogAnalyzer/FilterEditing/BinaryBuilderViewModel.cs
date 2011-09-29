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
			leftViewModel = ExpressionBuilderViewModelFactory.CreateViewModel( new DelegateBuilderProxy( builder, "Left" ), parameter );
			rightViewModel = ExpressionBuilderViewModelFactory.CreateViewModel( new DelegateBuilderProxy( builder, "Right" ), parameter );
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
