using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Windows.Markup;

namespace LogAnalyzer.Filters
{
	[Icon( "processor.png" )]
	[IgnoreBuilder]
	[ContentProperty( "ThreadId" )]
	public abstract class ThreadIdFilterBase : ExpressionBuilder
	{
		[FilterParameter( typeof( int ), "ThreadId" )]
		public int ThreadId
		{
			get { return Get<int>( "ThreadId" ); }
			set { Set( "ThreadId", value ); }
		}

		public sealed override Type GetResultType( ParameterExpression target )
		{
			return typeof( bool );
		}
	}

	public sealed class ThreadIdEquals : ThreadIdFilterBase
	{
		public ThreadIdEquals() { }
		public ThreadIdEquals( int threadId )
		{
			ThreadId = threadId;
		}

		protected override Expression CreateExpressionCore( ParameterExpression parameterExpression )
		{
			return Expression.Equal(
				Expression.Property( parameterExpression, "ThreadId" ),
				Expression.Constant( ThreadId, typeof( int ) )
				);
		}
	}

	public sealed class ThreadIdNotEquals : ThreadIdFilterBase
	{
		public ThreadIdNotEquals() { }
		public ThreadIdNotEquals( int threadId )
		{
			ThreadId = threadId;
		}

		protected override Expression CreateExpressionCore( ParameterExpression parameterExpression )
		{
			return
				Expression.NotEqual(
					Expression.Property( parameterExpression, "ThreadId" ),
					Expression.Constant( ThreadId, typeof( int ) )
				);
		}
	}

	[ContentProperty( "FileName" )]
	public abstract class FileNameFilterBase : ExpressionBuilder
	{
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
	}

	public sealed class FileNameEquals : FileNameFilterBase
	{
		public FileNameEquals() { }
		public FileNameEquals( string fileName )
		{
			if ( fileName == null )
				throw new ArgumentNullException( "fileName" );

			FileName = fileName;
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

	public sealed class FileNameNotEquals : FileNameFilterBase
	{
		public FileNameNotEquals() { }
		public FileNameNotEquals( string fileName )
		{
			if ( fileName == null )
				throw new ArgumentNullException( "fileName" );

			FileName = fileName;
		}

		protected override Expression CreateExpressionCore( ParameterExpression parameterExpression )
		{
			return
				Expression.NotEqual(
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

	[ContentProperty( "Pattern" )]
	public sealed class TextMatchesRegex : ExpressionBuilder
	{
		[FilterParameter( typeof( string ), "Pattern" )]
		public string Pattern
		{
			get { return Get<string>( "Pattern" ); }
			set { Set( "Pattern", value ); }
		}

		public override Type GetResultType( ParameterExpression target )
		{
			return typeof( bool );
		}

		protected override Expression CreateExpressionCore( ParameterExpression parameterExpression )
		{
			GetProperty getPropertyBuilder = new GetProperty { Target = new Argument(), PropertyName = "UnitedText" };
			var regex = new RegexMatchesFilterBuilder( Pattern, getPropertyBuilder );

			return regex.CreateExpression( parameterExpression );
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
