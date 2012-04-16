using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.Filters
{
	public sealed class IncludeFilesByNameFilter : FilesByNameFilterBase
	{
		protected override Expression CreateExpressionCore( ParameterExpression parameterExpression )
		{
			if ( parameterExpression.Type != typeof( IFileInfo ) )
			{
				throw new NotSupportedException( "IncludeFilesByNameFilter is for IFileInfo only." );
			}

			return Expression.Call(
				Expression.Constant( FileNames ), typeof( List<string> ).GetMethod( "Contains" ),
				Expression.Property( parameterExpression, "Name" ) );
		}
	}
}