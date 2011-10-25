using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Windows.Markup;

namespace LogAnalyzer.Filters
{
	[ContentProperty( "StringValue" )]
	public abstract class LogDateTimeFilterBase : ExpressionBuilder
	{
		public sealed override Type GetResultType( ParameterExpression target )
		{
			return typeof( bool );
		}
		
		public const string Format = "yyyy.MM.dd HH:mm:ss";

		public string StringValue
		{
			get { return Value.ToString( Format ); }
			set
			{
				DateTime dt;
				if ( DateTime.TryParseExact( value, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt ) )
				{
					Value = dt;
				}
			}
		}

		[FilterParameter( typeof( DateTime ), "Value" )]
		public DateTime Value
		{
			get { return Get<DateTime>( "Value" ); }
			set { Set( "Value", value ); }
		}
	}

	public sealed class LogDateTimeGreater : LogDateTimeFilterBase
	{
		public LogDateTimeGreater() { }
		public LogDateTimeGreater( DateTime value )
		{
			Value = value;
		}

		protected override Expression CreateExpressionCore( ParameterExpression parameterExpression )
		{
			GetProperty timeProperty = new GetProperty( new Argument(), "Time" );
			return Expression.GreaterThanOrEqual( timeProperty.CreateExpression( parameterExpression ), Expression.Constant( Value ) );
		}
	}

	public sealed class LogDateTimeLess : LogDateTimeFilterBase
	{
		public LogDateTimeLess() { }
		public LogDateTimeLess( DateTime value )
		{
			Value = value;
		}

		protected override Expression CreateExpressionCore(ParameterExpression parameterExpression)
		{
			GetProperty timeProperty = new GetProperty( new Argument(), "Time" );
			return Expression.LessThanOrEqual( timeProperty.CreateExpression( parameterExpression ), Expression.Constant( Value ) );
		}
	}
}
