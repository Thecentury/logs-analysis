using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using JetBrains.Annotations;

namespace LogAnalyzer.Filters
{
	/// <summary>
	/// Создает фильтр из переданного expression-а.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[IgnoreBuilder]
	public sealed class VerbatimExpressionBuilder<T> : ExpressionBuilder
	{
		private readonly Expression<Func<T, bool>> expression;

		public VerbatimExpressionBuilder( [NotNull] Expression<Func<T, bool>> expression )
		{
			if ( expression == null ) throw new ArgumentNullException( "expression" );
			this.expression = expression;
		}

		public override Type GetResultType( ParameterExpression target )
		{
			return typeof( T );
		}

		protected override Expression CreateExpressionCore( ParameterExpression parameterExpression )
		{
			return expression;
		}
	}
}
