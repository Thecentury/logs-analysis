using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.Filters
{
	public abstract class FilesByCleanedNameFilterBase : FilesByNameFilterBase
	{
		protected FilesByCleanedNameFilterBase() { }
		protected FilesByCleanedNameFilterBase( params string[] fileNames ) : base( fileNames ) { }

		protected Expression GetNameExpression( ParameterExpression parameterExpression )
		{
			if ( parameterExpression.Type == typeof( IFileInfo ) )
			{
				return Expression.Call( typeof( FileInfoExtensions ).GetMethod( "GetCleanedName" ), parameterExpression );
			}
			else
			{
				return Expression.Call( typeof( LogFileNameCleaner ).GetMethod( "GetCleanedName" ), parameterExpression );
			}
		}
	}

	public sealed class ExcludeFilesByCleanedNameFilter : FilesByCleanedNameFilterBase
	{
		public ExcludeFilesByCleanedNameFilter() { }
		public ExcludeFilesByCleanedNameFilter( params string[] fileNames ) : base( fileNames ) { }

		protected override Expression CreateExpressionCore2( ParameterExpression parameterExpression )
		{
			return
				Expression.Not(
					Expression.Call(
						Expression.Constant( FileNames ), typeof( HashSet<string> ).GetMethod( "Contains" ),
						GetNameExpression( parameterExpression ) ) );
		}
	}
}