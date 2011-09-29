using System;
using ExpressionBuilderSample;
using LogAnalyzer.Filters;
using System.Linq.Expressions;
using LogAnalyzer.GUI.FilterEditor;

namespace LogAnalyzer.GUI.FilterEditing
{
	internal sealed class ExpressionBuilderViewModelFactory
	{
		public static ExpressionBuilderViewModel CreateViewModel( ExpressionBuilder builder, ParameterExpression parameter )
		{
			if ( builder == null )
				throw new ArgumentNullException( "builder" );
			if ( parameter == null )
				throw new ArgumentNullException( "parameter" );

			BinaryExpressionBuilder binaryBuilder = builder as BinaryExpressionBuilder;
			if ( binaryBuilder != null )
				return new BinaryBuilderViewModel( binaryBuilder, parameter );

			StringFilterBuilder stringFilterBuilder = builder as StringFilterBuilder;
			if ( stringFilterBuilder != null )
				return new StringFilterBuilderViewModel( stringFilterBuilder, parameter );

			GetProperty getPropertyBuilder = builder as GetProperty;
			if ( getPropertyBuilder != null )
				return new GetPropertyBuilderViewModel( getPropertyBuilder, parameter );

			DelegateBuilderProxy delegateBuilder = builder as DelegateBuilderProxy;
			if ( delegateBuilder != null )
				return new DelegateBuilderViewModel( delegateBuilder, parameter );

			Not notBuilder = builder as Not;
			if ( notBuilder != null )
				return new NotBuilderViewModel( notBuilder, parameter );

			return new ExpressionBuilderViewModel( builder, parameter );
		}
	}
}
