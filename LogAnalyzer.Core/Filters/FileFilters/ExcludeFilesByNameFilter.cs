using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.Filters
{
	public sealed class ExcludeFilesByNameFilter : FilesByNameFilterBase
	{
		protected override Expression CreateExpressionCore( ParameterExpression parameterExpression )
		{
			if ( parameterExpression.Type != typeof( IFileInfo ) )
			{
				throw new NotSupportedException( "ExcludeFilesByNameFilter is for IFileInfo only." );
			}

			return
				Expression.Not(
					Expression.Call(
						Expression.Constant( FileNames ), typeof( List<string> ).GetMethod( "Contains" ),
						Expression.Property( parameterExpression, "Name" ) )
					);
		}
	}

	public sealed class ExcludeFilesByCleanedNameFilter : FilesByNameFilterBase
	{
		public ExcludeFilesByCleanedNameFilter() { }
		public ExcludeFilesByCleanedNameFilter( params string[] fileNames ) : base( fileNames ) { }

		protected override Expression CreateExpressionCore( ParameterExpression parameterExpression )
		{
			if ( parameterExpression.Type != typeof( IFileInfo ) )
			{
				throw new NotSupportedException( "ExcludeFilesByCleanedNameFilter is for IFileInfo only." );
			}

			return
				Expression.Not(
					Expression.Call(
						Expression.Constant( FileNames ), typeof( List<string> ).GetMethod( "Contains" ),
						Expression.Call( typeof( FileInfoExtensions ).GetMethod( "GetCleanedName" ), parameterExpression ) ) );
		}
	}
}