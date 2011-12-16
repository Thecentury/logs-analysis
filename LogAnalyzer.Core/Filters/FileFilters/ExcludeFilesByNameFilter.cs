using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows.Markup;
using LogAnalyzer.Kernel;

// ReSharper disable CheckNamespace
namespace LogAnalyzer.Filters
// ReSharper restore CheckNamespace
{
	public sealed class ExcludeFilesByNameFilter : FilesByNameFilterBase
	{
		protected override Expression CreateExpressionCore( ParameterExpression parameterExpression )
		{
			if ( parameterExpression.Type != typeof( IFileInfo ) )
				throw new NotSupportedException( "ExcludeFilesByNameFilter is for IFileInfo only." );

			return
				Expression.Not(
					Expression.Call(
					Expression.Constant( FileNames ), typeof( List<string> ).GetMethod( "Contains" ),
					Expression.Property( parameterExpression, "Name" ) )
					);
		}
	}
}