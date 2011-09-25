using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.ComponentModel;
using System.Windows.Markup;

namespace LogAnalyzer.Filters
{
	[ContentProperty( "Children" )]
	public abstract class BinaryExpressionBuilder : ExpressionBuilder, ISupportInitialize
	{
		protected BinaryExpressionBuilder()
		{
			children.Add( null );
			children.Add( null );
		}

		protected BinaryExpressionBuilder( ExpressionBuilder left, ExpressionBuilder right )
			: this()
		{
			if ( left == null )
				throw new ArgumentNullException( "left" );
			if ( right == null )
				throw new ArgumentNullException( "right" );

			Left = left;
			Right = right;
		}

		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		[FilterParameter( typeof( ExpressionBuilder ), "Left" )]
		public ExpressionBuilder Left
		{
			get { return GetExpressionBuilder( "Left" ); }
			set
			{
				children[0] = value;
				Set( "Left", value );
			}
		}

		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		[FilterParameter( typeof( ExpressionBuilder ), "Right" )]
		public ExpressionBuilder Right
		{
			get { return GetExpressionBuilder( "Right" ); }
			set
			{
				children[1] = value;
				Set( "Right", value );
			}
		}

		public readonly List<ExpressionBuilder> children = new List<ExpressionBuilder>( 2 );
		[EditorBrowsable( EditorBrowsableState.Never )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Content )]
		public List<ExpressionBuilder> Children
		{
			get { return children; }
		}

		protected sealed override Expression CreateExpressionCore( ParameterExpression parameterExpression )
		{
			Expression leftExpression = Left.CreateExpression( parameterExpression );
			Expression rightExpression = Right.CreateExpression( parameterExpression );

			Expression result = CreateBinaryCore( leftExpression, rightExpression );
			return result;
		}

		protected abstract BinaryExpression CreateBinaryCore( Expression left, Expression right );

		public static T Create<T>( ExpressionBuilder left, ExpressionBuilder right ) where T : BinaryExpressionBuilder, new()
		{
			if ( left == null )
				throw new ArgumentNullException( "left" );
			if ( right == null )
				throw new ArgumentNullException( "right" );

			T instance = new T { Left = left, Right = right };
			return instance;
		}

		#region ISupportInitialize Members

		void ISupportInitialize.BeginInit()
		{

		}

		void ISupportInitialize.EndInit()
		{
			Left = Children[0];
			Right = Children[1];
		}

		#endregion
	}
}
