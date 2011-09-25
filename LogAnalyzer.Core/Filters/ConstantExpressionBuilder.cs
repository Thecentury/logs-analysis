using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace LogAnalyzer.Filters
{
	public static class ConstantBuilder
	{
		public static IntConstant Create( int value )
		{
			return new IntConstant( value );
		}

		public static StringConstant Create( string value )
		{
			return new StringConstant( value );
		}

		public static DoubleConstant Create( double value )
		{
			return new DoubleConstant( value );
		}

		public static DateTimeConstant Create( DateTime value )
		{
			return new DateTimeConstant( value );
		}

		public static TimeSpanConstant Create( TimeSpan value )
		{
			return new TimeSpanConstant( value );
		}

		public static BoolConstant Create( bool value )
		{
			return new BoolConstant( value );
		}
	}

	[IgnoreBuilder]
	public abstract class IntermediateConstantBuilder : ExpressionBuilder { }

	[IgnoreBuilder]
	public class ConstantExpressionBuilder<TConstant> : IntermediateConstantBuilder
	{
		public ConstantExpressionBuilder() { }

		public ConstantExpressionBuilder( TConstant value )
		{
			Value = value;
		}

		[FilterParameter( typeof( object ), "Value" )]
		public TConstant Value
		{
			get { return Get<TConstant>( "Value" ); }
			set { Set( "Value", value ); }
		}

		public override Type GetResultType( ParameterExpression target )
		{
			return typeof( TConstant );
		}

		protected override Expression CreateExpressionCore( ParameterExpression parameterExpression )
		{
			return Expression.Constant( Value, typeof( TConstant ) );
		}
	}

	public sealed class IntConstant : ConstantExpressionBuilder<int>
	{
		public IntConstant() { }
		public IntConstant( int value ) { Value = value; }
	}

	public sealed class StringConstant : ConstantExpressionBuilder<string>
	{
		public StringConstant() { }
		public StringConstant( string value ) { Value = value; }
	}

	public sealed class DoubleConstant : ConstantExpressionBuilder<double>
	{
		public DoubleConstant() { }
		public DoubleConstant( double value ) { Value = value; }
	}

	public sealed class DateTimeConstant : ConstantExpressionBuilder<DateTime>
	{
		public DateTimeConstant() { }
		public DateTimeConstant( DateTime value ) { Value = value; }
	}

	public sealed class TimeSpanConstant : ConstantExpressionBuilder<TimeSpan>
	{
		public TimeSpanConstant() { }
		public TimeSpanConstant( TimeSpan value ) { Value = value; }
	}

	public sealed class BoolConstant : ConstantExpressionBuilder<bool>
	{
		public BoolConstant() { }
		public BoolConstant( bool value ) { Value = value; }
	}
}
