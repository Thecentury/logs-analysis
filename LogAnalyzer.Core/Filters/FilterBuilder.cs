using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace LogAnalyzer.Filters
{
	public static class FilterBuilder
	{
		public static IFilter<LogEntry> BuildLogEntryFilter( Func<ParameterExpression, Expression> createExpressionHandler )
		{
			return BuildFilter<LogEntry>( createExpressionHandler );
		}

		public static IFilter<T> BuildFilter<T>( Func<ParameterExpression, Expression> createExpressionHandler )
		{
			Func<T, bool> predicate = Compile<T>( createExpressionHandler );

			return new DelegateFilter<T>( predicate );
		}

		public static Func<T, bool> Compile<T>( Func<ParameterExpression, Expression> createExpressionHandler )
		{
			ParameterExpression parameter = Expression.Parameter( typeof( T ) );
			Expression expression = createExpressionHandler( parameter );

			Expression<Func<T, bool>> lambda;
			if ( expression is Expression<Func<T, bool>> )
			{
				lambda = (Expression<Func<T, bool>>)expression;
			}
			else
			{
				if (expression is LambdaExpression)
				{
					expression = ((LambdaExpression) expression).Body;
				}
				lambda = Expression.Lambda<Func<T, bool>>( expression, parameter );
			}

			Func<T, bool> predicate = lambda.Compile();
			return predicate;
		}
	}
}
