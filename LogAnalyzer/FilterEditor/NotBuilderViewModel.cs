
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Filters;
using System.Linq.Expressions;

namespace ExpressionBuilderSample
{
	internal sealed class NotBuilderViewModel : ExpressionBuilderViewModel
	{
		private readonly Not notBuilder;
		private readonly ExpressionBuilderViewModel innerViewModel = null;

		public NotBuilderViewModel( Not notBuilder, ParameterExpression parameter )
			: base( notBuilder, parameter )
		{
			this.notBuilder = notBuilder;
			innerViewModel = ExpressionBuilderViewModelFactory.CreateViewModel( new DelegateBuilderProxy( notBuilder, "Inner" ), parameter );
		}

		public ExpressionBuilderViewModel Inner
		{
			get { return innerViewModel; }
		}

		protected override void OnSelectedChildChanged( ExpressionBuilder builder )
		{
			notBuilder.Inner = builder;
		}
	}
}
