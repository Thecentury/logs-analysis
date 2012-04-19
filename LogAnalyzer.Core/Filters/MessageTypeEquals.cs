using System;
using System.Linq.Expressions;
using System.Windows.Markup;

namespace LogAnalyzer.Filters
{
	[Icon( "burn.png" )]
	[ContentProperty( "Value" )]
	public sealed class MessageTypeEquals : ExpressionBuilder
	{
		public MessageTypeEquals() { }
		public MessageTypeEquals( string value )
		{
			Value = value;
		}

		[FilterParameter( typeof( string ), "Value" )]
		public string Value
		{
			get { return Get<string>( "Value" ); }
			set { Set( "Value", value ); }
		}

		public override Type GetResultType( ParameterExpression target )
		{
			return typeof( bool );
		}

		protected override Expression CreateExpressionCore( ParameterExpression parameterExpression )
		{
			GetProperty getPropertyBuilder = new GetProperty( new Argument(), "Type" );
			Equals equalsBuilder = new Equals( getPropertyBuilder, new StringConstant( Value ) );

			return equalsBuilder.CreateExpression( parameterExpression );
		}
	}
}