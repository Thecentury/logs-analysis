using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Filters;
using System.Linq.Expressions;

namespace ExpressionBuilderSample
{
	internal class BinaryBuilderViewModel : ExpressionBuilderViewModel
	{
		private readonly ExpressionBuilderViewModel leftViewModel = null;
		private readonly ExpressionBuilderViewModel rightViewModel = null;

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
