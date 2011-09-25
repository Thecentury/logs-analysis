using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace LogAnalyzer.Filters
{
	public sealed class AlwaysFalse : StaticBuilder
	{
		public override Type GetResultType( ParameterExpression target )
		{
			return typeof( bool );
		}

		protected override Expression CreateExpressionCore( ParameterExpression parameterExpression )
		{
			return Expression.Constant( false, typeof( bool ) );
		}
	}
}
