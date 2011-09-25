using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace LogAnalyzer.Filters
{
	public sealed class Argument : StaticBuilder
	{
		public override Type GetResultType( ParameterExpression target )
		{
			return target.Type;
		}

		protected override Expression CreateExpressionCore( ParameterExpression parameterExpression )
		{
			return parameterExpression;
		}
	}
}
