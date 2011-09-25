using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Windows.Markup;

namespace LogAnalyzer.Filters
{
	[ContentProperty( "ThreadId" )]
	public sealed class ThreadIdEquals : ExpressionBuilder
	{
		public ThreadIdEquals() { }
		public ThreadIdEquals( int threadId )
		{
			ThreadId = threadId;
		}

		[FilterParameter( typeof( int ), "ThreadId" )]
		public int ThreadId
		{
			get { return Get<int>( "ThreadId" ); }
			set { Set( "ThreadId", value ); }
		}

		public override Type GetResultType( ParameterExpression target )
		{
			return typeof( bool );
		}

		protected override Expression CreateExpressionCore( ParameterExpression parameterExpression )
		{
			return Expression.Equal(
				Expression.Property( parameterExpression, "ThreadId" ),
				Expression.Constant( ThreadId, typeof( int ) )
				);
		}
	}

	[ContentProperty( "FileName" )]
	public sealed class FileNameEquals : ExpressionBuilder
	{
		public FileNameEquals() { }
		public FileNameEquals( string fileName )
		{
			if ( fileName == null )
				throw new ArgumentNullException( "fileName" );

			this.FileName = fileName;
		}

		[FilterParameter( typeof( string ), "FileName" )]
		public string FileName
		{
			get { return Get<string>( "FileName" ); }
			set { Set( "FileName", value ); }
		}

		public override Type GetResultType( ParameterExpression target )
		{
			return typeof( bool );
		}

		protected override Expression CreateExpressionCore( ParameterExpression parameterExpression )
		{
			return Expression.Equal(
				Expression.Property(
					Expression.Property( parameterExpression, "ParentLogFile" ),
					"Name" ),
				Expression.Constant( FileName, typeof( string ) )
				);
		}
	}

	[ContentProperty( "Substring" )]
	public sealed class TextContains : ExpressionBuilder
	{
		[FilterParameter( typeof( string ), "Substring" )]
		public string Substring
		{
			get { return Get<string>( "Substring" ); }
			set { Set( "Substring", value ); }
		}

		public override Type GetResultType( ParameterExpression target )
		{
			return typeof( bool );
		}

		protected override Expression CreateExpressionCore( ParameterExpression parameterExpression )
		{
			GetProperty getPropertyBuilder = new GetProperty { Target = new Argument(), PropertyName = "UnitedText" };
			StringContains containsBuilder = new StringContains { Inner = getPropertyBuilder, Substring = new StringConstant { Value = Substring } };

			return containsBuilder.CreateExpression( parameterExpression );
		}
	}

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
