using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.Filters
{
	public sealed class IncludeFilesByCleanedNameFilter : FilesByNameFilterBase
	{
		public IncludeFilesByCleanedNameFilter() { }
		public IncludeFilesByCleanedNameFilter( params string[] fileNames ) : base( fileNames ) { }

		protected override Expression CreateExpressionCore( ParameterExpression parameterExpression )
		{
			if ( parameterExpression.Type != typeof( IFileInfo ) )
			{
				throw new NotSupportedException( "IncludeFilesByCleanedNameFilter is for IFileInfo only." );
			}

			return Expression.Call(
				Expression.Constant( FileNames ), typeof( HashSet<string> ).GetMethod( "Contains" ),
				Expression.Call( typeof( FileInfoExtensions ).GetMethod( "GetCleanedName" ), parameterExpression ) );
		}
	}
}