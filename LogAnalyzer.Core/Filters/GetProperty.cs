using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Markup;

namespace LogAnalyzer.Filters
{
	[ContentProperty( "Target" )]
	public sealed class GetProperty : ExpressionBuilder
	{
		public GetProperty() { }

		public GetProperty( ExpressionBuilder target, string propertyName )
		{
			if ( target == null )
				throw new ArgumentNullException( "target" );
			if ( propertyName == null )
				throw new ArgumentNullException( "propertyName" );

			this.Target = target;
			this.PropertyName = propertyName;
		}

		[FilterParameter( typeof( string ), "PropertyName", ParameterReturnType = typeof( string ) )]
		public string PropertyName
		{
			get { return Get<string>( "PropertyName" ); }
			set { Set( "PropertyName", value ); }
		}

		[FilterParameter( typeof( ExpressionBuilder ), "Target" )]
		public ExpressionBuilder Target
		{
			get { return Get<ExpressionBuilder>( "Target" ); }
			set { Set( "Target", value ); }
		}

		protected override Expression CreateExpressionCore( ParameterExpression parameterExpression )
		{
			Expression targetExpression = Target.CreateExpression( parameterExpression );
			return Expression.Property( targetExpression, PropertyName );
		}

		public override Type GetResultType( ParameterExpression target )
		{
			if ( HasValue( "PropertyName" ) )
			{
				MemberExpression memberExpression = (MemberExpression)CreateExpression( target );
				return ((PropertyInfo)memberExpression.Member).PropertyType;
			}
			else
			{
				return typeof( object );
			}
		}
	}
}
