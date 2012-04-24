﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Markup;
using LogAnalyzer.Extensions;

namespace LogAnalyzer.Filters
{
	[ContentProperty( "Children" )]
	public abstract class BooleanCollectionBuilder : ExpressionBuilder
	{
		protected BooleanCollectionBuilder()
		{
			_children.CollectionChanged += OnChildrenCollectionChanged;
		}

		private void OnChildrenCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			PropertyChangedDelegate.Raise( this, "Children" );
		}

		private readonly ObservableCollection<ExpressionBuilder> _children = new ObservableCollection<ExpressionBuilder>();
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Content )]
		public Collection<ExpressionBuilder> Children
		{
			get { return _children; }
		}

		public sealed override Type GetResultType( ParameterExpression target )
		{
			return typeof( bool );
		}

		protected sealed override bool ValidatePropertiesCore()
		{
			return _children.Count >= 2
				   && _children.All( c => c != null )
				   && _children.All( c => c.ValidateProperties() );
		}

		protected sealed override Expression CreateExpressionCore( ParameterExpression parameterExpression )
		{
			var second = _children.Last().CreateExpression( parameterExpression );

			for ( int i = _children.Count - 2; i >= 0; i-- )
			{
				var first = _children[i].CreateExpression( parameterExpression );
				var op = CreateBinaryOperation( first, second );
				second = op;
			}

			return second;
		}

		protected abstract Expression CreateBinaryOperation( Expression left, Expression right );
	}

	public sealed class AndCollection : BooleanCollectionBuilder
	{
		public AndCollection() { }

		public AndCollection( params ExpressionBuilder[] children )
		{
			Children.AddRange( children );
		}

		protected override Expression CreateBinaryOperation( Expression left, Expression right )
		{
			return Expression.AndAlso( left, right );
		}
	}

	public sealed class OrCollection : BooleanCollectionBuilder
	{
		public OrCollection() { }

		public OrCollection( params ExpressionBuilder[] children )
		{
			Children.AddRange( children );
		}

		protected override Expression CreateBinaryOperation( Expression left, Expression right )
		{
			return Expression.OrElse( left, right );
		}
	}
}
