using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Windows.Markup;

namespace LogAnalyzer.Filters
{
	[ContentProperty( "Inner" )]
	public sealed class Not : ExpressionBuilder
	{
		[FilterParameter( typeof( ExpressionBuilder ), "Inner", ParameterReturnType = typeof( bool ) )]
		public ExpressionBuilder Inner
		{
			get { return Get<ExpressionBuilder>( "Inner" ); }
			set { Set( "Inner", value ); }
		}

		protected override bool ValidatePropertiesCore()
		{
			return Inner.ValidateProperties();
		}

		public override Type GetResultType( ParameterExpression target )
		{
			return typeof( bool );
		}

		protected override Expression CreateExpressionCore( ParameterExpression parameterExpression )
		{
			return Expression.Not( Inner.CreateExpression( parameterExpression ) );
		}
	}

	public abstract class BooleanBinaryExpressionBuilder : BinaryExpressionBuilder, IOverridePropertyTypeInfo
	{
		protected BooleanBinaryExpressionBuilder() { }
		protected BooleanBinaryExpressionBuilder( ExpressionBuilder left, ExpressionBuilder right ) : base( left, right ) { }

		public sealed override Type GetResultType( ParameterExpression target )
		{
			return typeof( bool );
		}

		#region IOverridePropertyTypeInfo Members

		Type IOverridePropertyTypeInfo.GetPropertyType( string propertyName )
		{
			if ( !(propertyName == "Left" || propertyName == "Right") )
				throw new ArgumentException( "Invalid property name." );

			return typeof( bool );
		}

		#endregion
	}

	public sealed class GreaterThan : BooleanBinaryExpressionBuilder
	{
		public GreaterThan() { }
		public GreaterThan( ExpressionBuilder left, ExpressionBuilder right ) : base( left, right ) { }

		protected override BinaryExpression CreateBinaryCore( Expression left, Expression right )
		{
			return Expression.GreaterThan( left, right );
		}
	}

	public sealed class GreaterThanOrEqual : BooleanBinaryExpressionBuilder
	{
		public GreaterThanOrEqual() { }
		public GreaterThanOrEqual( ExpressionBuilder left, ExpressionBuilder right ) : base( left, right ) { }

		protected override BinaryExpression CreateBinaryCore( Expression left, Expression right )
		{
			return Expression.GreaterThanOrEqual( left, right );
		}
	}

	public sealed class LessThan : BooleanBinaryExpressionBuilder
	{
		public LessThan() { }
		public LessThan( ExpressionBuilder left, ExpressionBuilder right ) : base( left, right ) { }

		protected override BinaryExpression CreateBinaryCore( Expression left, Expression right )
		{
			return Expression.LessThan( left, right );
		}
	}

	public sealed class LessThanOrEqual : BooleanBinaryExpressionBuilder
	{
		public LessThanOrEqual() { }
		public LessThanOrEqual( ExpressionBuilder left, ExpressionBuilder right ) : base( left, right ) { }

		protected override BinaryExpression CreateBinaryCore( Expression left, Expression right )
		{
			return Expression.LessThanOrEqual( left, right );
		}
	}

	public sealed class Equals : BooleanBinaryExpressionBuilder
	{
		public Equals() { }
		public Equals( ExpressionBuilder left, ExpressionBuilder right ) : base( left, right ) { }

		protected override BinaryExpression CreateBinaryCore( Expression left, Expression right )
		{
			return Expression.Equal( left, right );
		}
	}

	public sealed class NotEquals : BooleanBinaryExpressionBuilder
	{
		public NotEquals() { }
		public NotEquals( ExpressionBuilder left, ExpressionBuilder right ) : base( left, right ) { }

		protected override BinaryExpression CreateBinaryCore( Expression left, Expression right )
		{
			return Expression.NotEqual( left, right );
		}
	}

	public sealed class And : BooleanBinaryExpressionBuilder
	{
		public And() { }
		public And( ExpressionBuilder left, ExpressionBuilder right ) : base( left, right ) { }

		protected override BinaryExpression CreateBinaryCore( Expression left, Expression right )
		{
			return Expression.AndAlso( left, right );
		}
	}

	public sealed class Or : BooleanBinaryExpressionBuilder
	{
		public Or() { }
		public Or( ExpressionBuilder left, ExpressionBuilder right ) : base( left, right ) { }

		protected override BinaryExpression CreateBinaryCore( Expression left, Expression right )
		{
			return Expression.OrElse( left, right );
		}
	}

}
