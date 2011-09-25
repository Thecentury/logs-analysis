using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using LogAnalyzer.Filters;

namespace ExpressionBuilderSample
{
	internal sealed class GetPropertyBuilderViewModel : ExpressionBuilderViewModel
	{
		private readonly ExpressionBuilderViewModel targetViewModel = null;
		private readonly GetProperty getPropertyBuilder = null;

		public GetPropertyBuilderViewModel( GetProperty builder, ParameterExpression parameter )
			: base( builder, parameter )
		{
			this.getPropertyBuilder = builder;
			targetViewModel = ExpressionBuilderViewModelFactory.CreateViewModel( new DelegateBuilderProxy( builder, "Target" ), parameter );
		}

		public ExpressionBuilderViewModel Target
		{
			get { return targetViewModel; }
		}

		protected override void OnSelectedChildChanged( ExpressionBuilder builder )
		{
			getPropertyBuilder.Target = builder;
		}
	}
}
