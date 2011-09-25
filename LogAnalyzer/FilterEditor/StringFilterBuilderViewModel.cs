using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Filters;
using System.Linq.Expressions;

namespace ExpressionBuilderSample
{
	internal class StringFilterBuilderViewModel : ExpressionBuilderViewModel
	{
		private readonly ExpressionBuilderViewModel stringViewModel = null;
		private readonly ExpressionBuilderViewModel substringViewModel = null;

		public StringFilterBuilderViewModel( StringFilterBuilder builder, ParameterExpression parameter )
			: base( builder, parameter )
		{
			stringViewModel = ExpressionBuilderViewModelFactory.CreateViewModel( new DelegateBuilderProxy( builder, "Inner" ), parameter );
			substringViewModel = ExpressionBuilderViewModelFactory.CreateViewModel( new DelegateBuilderProxy( builder, "Substring" ), parameter );
		}

		public ExpressionBuilderViewModel String
		{
			get { return stringViewModel; }
		}

		public ExpressionBuilderViewModel Substring
		{
			get { return substringViewModel; }
		}
	}
}
