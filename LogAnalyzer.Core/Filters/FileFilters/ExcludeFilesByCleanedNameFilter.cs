using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.Filters
{
	public sealed class ExcludeFilesByCleanedNameFilter : FilesByNameFilterBase
	{
		public ExcludeFilesByCleanedNameFilter() { }
		public ExcludeFilesByCleanedNameFilter( params string[] fileNames ) : base( fileNames ) { }

		protected override Expression CreateExpressionCore2( ParameterExpression parameterExpression )
		{
			if ( parameterExpression.Type != typeof( IFileInfo ) )
			{
				throw new NotSupportedException( "ExcludeFilesByCleanedNameFilter is for IFileInfo only." );
			}

			return
				Expression.Not(
					Expression.Call(
						Expression.Constant( FileNames ), typeof( HashSet<string> ).GetMethod( "Contains" ),
						Expression.Call( typeof( FileInfoExtensions ).GetMethod( "GetCleanedName" ), parameterExpression ) ) );
		}
	}
}