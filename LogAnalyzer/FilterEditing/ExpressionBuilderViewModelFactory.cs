using System;
using JetBrains.Annotations;
using LogAnalyzer.Filters;
using System.Linq.Expressions;
using LogAnalyzer.GUI.FilterEditor;

namespace LogAnalyzer.GUI.FilterEditing
{
	internal static class ExpressionBuilderViewModelFactory
	{
		public static ExpressionBuilderViewModel CreateViewModel( [NotNull] BuilderContext ctx )
		{
			if ( ctx == null )
			{
				throw new ArgumentNullException( "ctx" );
			}

			var builder = ctx.Builder;

			BinaryExpressionBuilder binaryBuilder = builder as BinaryExpressionBuilder;
			if ( binaryBuilder != null )
				return new BinaryBuilderViewModel( ctx.WithBuilder( binaryBuilder ) );

			StringFilterBuilder stringFilterBuilder = builder as StringFilterBuilder;
			if ( stringFilterBuilder != null )
				return new StringFilterBuilderViewModel( ctx.WithBuilder( stringFilterBuilder ) );

			GetProperty getPropertyBuilder = builder as GetProperty;
			if ( getPropertyBuilder != null )
				return new GetPropertyBuilderViewModel( ctx.WithBuilder( getPropertyBuilder ) );

			DelegateBuilderProxy delegateBuilder = builder as DelegateBuilderProxy;
			if ( delegateBuilder != null )
				return new DelegateBuilderViewModel( ctx.WithBuilder( delegateBuilder ) );

			Not notBuilder = builder as Not;
			if ( notBuilder != null )
				return new NotBuilderViewModel( ctx.WithBuilder( notBuilder ) );

			LogDateTimeFilterBase logDateTime = builder as LogDateTimeFilterBase;
			if ( logDateTime != null )
				return new LogDateTimeViewModel( ctx.WithBuilder( logDateTime ) );

			BooleanCollectionBuilder booleanCollectionBuilder = builder as BooleanCollectionBuilder;
			if ( booleanCollectionBuilder != null )
				return new CollectionBooleanBuilderViewModel( ctx.WithBuilder( booleanCollectionBuilder ) );

			ProxyCollectionElementBuilder collectionProxy = builder as ProxyCollectionElementBuilder;
			if ( collectionProxy != null )
				return new CollectionBooleanChildBuilderViewModel( ctx.WithBuilder( collectionProxy ) );

			return new ExpressionBuilderViewModel( ctx );
		}
	}
}
