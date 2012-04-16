using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.Filters
{
	public sealed class IncludeFilesByCleanedNameFilter : FilesByCleanedNameFilterBase
	{
		public IncludeFilesByCleanedNameFilter() { }
		public IncludeFilesByCleanedNameFilter( params string[] fileNames ) : base( fileNames ) { }

		protected override Expression CreateExpressionCore2( ParameterExpression parameterExpression )
		{
			return Expression.Call(
				Expression.Constant( FileNames ), typeof( HashSet<string> ).GetMethod( "Contains" ),
				GetNameExpression( parameterExpression ) );
		}
	}
}