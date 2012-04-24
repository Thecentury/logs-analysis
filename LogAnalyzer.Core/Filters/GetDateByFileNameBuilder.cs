using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LogAnalyzer.Filters
{
	public sealed class GetDateByFileNameBuilder : ExpressionBuilder
	{
		public override Type GetResultType( ParameterExpression target )
		{
			return typeof( DateTime );
		}

		protected override Expression CreateExpressionCore( ParameterExpression parameterExpression )
		{
			if ( parameterExpression.Type == typeof( string ) )
			{

			}
			throw new NotImplementedException();
		}
	}
}
