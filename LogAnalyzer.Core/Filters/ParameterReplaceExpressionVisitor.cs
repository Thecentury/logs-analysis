using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace LogAnalyzer.Filters
{
	public sealed class ParameterReplaceExpressionVisitor : ExpressionVisitor
	{
		private readonly ParameterExpression _parameter;

		public ParameterReplaceExpressionVisitor( [NotNull] ParameterExpression parameter )
		{
			if ( parameter == null )
			{
				throw new ArgumentNullException( "parameter" );
			}
			_parameter = parameter;
		}

		protected override Expression VisitParameter( ParameterExpression node )
		{
			return _parameter;
		}
	}
}