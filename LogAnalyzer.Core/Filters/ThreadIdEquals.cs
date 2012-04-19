using System;
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
}