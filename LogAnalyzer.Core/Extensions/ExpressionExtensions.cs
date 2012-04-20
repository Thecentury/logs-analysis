using System.Linq.Expressions;
using LogAnalyzer.Filters;

namespace LogAnalyzer.Extensions
{
	public static class ExpressionExtensions
	{
		public static Expression ReplaceParameter( this Expression expression, ParameterExpression parameter )
		{
			ParameterReplaceExpressionVisitor visitor = new ParameterReplaceExpressionVisitor( parameter );

			var result = visitor.Visit( expression );
			return result;
		}
	}
}