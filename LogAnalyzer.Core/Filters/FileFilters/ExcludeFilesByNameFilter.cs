using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.Filters
{
	public sealed class ExcludeFilesByNameFilter : FilesByNameFilterBase
	{
		protected override Expression CreateExpressionCore2( ParameterExpression parameterExpression )
		{
			return
				Expression.Not(
					Expression.Call(
						Expression.Constant( FileNames ), typeof( HashSet<string> ).GetMethod( "Contains" ),
						Expression.Property( parameterExpression, "Name" ) )
					);
		}
	}
}