using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using LogAnalyzer.Extensions;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.Filters
{
	[Icon( "calendar-day.png" )]
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
				return CreateDateTimeExpression<string>( s => LogFileNameCleaner.GetDate( s ), parameterExpression );
			}
			if ( parameterExpression.Type == typeof( LogFile ) )
			{
				return CreateDateTimeExpression<LogFile>( f => LogFileNameCleaner.GetDate( f.Name ), parameterExpression );
			}
			if ( parameterExpression.Type == typeof( IFileInfo ) )
			{
				return CreateDateTimeExpression<IFileInfo>( f => LogFileNameCleaner.GetDate( f.Name ), parameterExpression );
			}
			if ( parameterExpression.Type == typeof( LogEntry ) )
			{
				return CreateDateTimeExpression<LogEntry>( le => LogFileNameCleaner.GetDate( le.ParentLogFile.Name ),
														  parameterExpression );
			}

			throw new NotSupportedException( String.Format( "Unexpected parameter type {0}", parameterExpression.Type ) );
		}

		private Expression CreateDateTimeExpression<T>( Expression<Func<T, DateTime>> expression, ParameterExpression parameter )
		{
			return expression.ReplaceParameter( parameter );
		}
	}
}
