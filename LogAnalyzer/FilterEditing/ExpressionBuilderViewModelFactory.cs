using System;
using LogAnalyzer.Filters;
using System.Linq.Expressions;
using LogAnalyzer.GUI.FilterEditor;

namespace LogAnalyzer.GUI.FilterEditing
{
	internal static class ExpressionBuilderViewModelFactory
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

			LogDateTimeFilterBase logDateTime = builder as LogDateTimeFilterBase;
			if ( logDateTime != null )
				return new LogDateTimeViewModel( logDateTime, parameter );

			BooleanCollectionBuilder booleanCollectionBuilder = builder as BooleanCollectionBuilder;
			if ( booleanCollectionBuilder != null )
				return new CollectionBooleanBuilderViewModel( booleanCollectionBuilder, parameter );

			ProxyCollectionElementBuilder collectionProxy = builder as ProxyCollectionElementBuilder;
			if ( collectionProxy != null )
				return new CollectionBooleanChildBuilderViewModel( collectionProxy, parameter );

			return new ExpressionBuilderViewModel( builder, parameter );
		}
	}
}
