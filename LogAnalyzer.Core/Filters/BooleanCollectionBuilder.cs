﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Markup;

namespace LogAnalyzer.Filters
{
	[ContentProperty( "Children" )]
	public abstract class BooleanCollectionBuilder : ExpressionBuilder
	{
		private readonly Collection<ExpressionBuilder> children = new Collection<ExpressionBuilder>();
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Content )]
		public Collection<ExpressionBuilder> Children
		{
			get { return children; }
		}

		public sealed override Type GetResultType( ParameterExpression target )
		{
			return typeof( bool );
		}

		protected override bool ValidatePropertiesCore()
		{
			return children.Count >= 2 && children.All( c => c != null );
		}

		protected sealed override Expression CreateExpressionCore( ParameterExpression parameterExpression )
		{
			var second = children.Last().CreateExpression( parameterExpression );

			for ( int i = children.Count - 2; i >= 0; i-- )
			{
				var first = children[i].CreateExpression( parameterExpression );
				var op = CreateBinaryOperation( first, second );
				second = op;
			}

			return second;
		}

		protected abstract Expression CreateBinaryOperation( Expression left, Expression right );
	}

	public sealed class AndCollection : BooleanCollectionBuilder
	{
		protected override Expression CreateBinaryOperation( Expression left, Expression right )
		{
			return Expression.AndAlso( left, right );
		}
	}

	public sealed class OrCollection : BooleanCollectionBuilder
	{
		protected override Expression CreateBinaryOperation( Expression left, Expression right )
		{
			return Expression.OrElse( left, right );
		}
	}
}
