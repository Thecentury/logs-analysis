using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.Filters
{
	public abstract class FilesByNotCleanedNameFilterBase : FilesByNameFilterBase
	{
		protected Expression GetFileName( ParameterExpression parameter )
		{
			if ( parameter.Type == typeof( IFileInfo ) )
			{
				return Expression.Property( parameter, "Name" );
			}
			else
			{
				return parameter;
			}
		}
	}

	public sealed class IncludeFilesByNameFilter : FilesByNotCleanedNameFilterBase
	{
		protected override Expression CreateExpressionCore2( ParameterExpression parameterExpression )
		{
			return Expression.Call(
				Expression.Constant( FileNames ), typeof( HashSet<string> ).GetMethod( "Contains" ),
				GetFileName( parameterExpression ) );
		}
	}
}