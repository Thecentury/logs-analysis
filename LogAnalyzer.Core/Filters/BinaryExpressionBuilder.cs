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
		protected BinaryExpressionBuilder() { }

		protected BinaryExpressionBuilder( ExpressionBuilder left, ExpressionBuilder right )
			: this()
		{
			if ( left == null )
				throw new ArgumentNullException( "left" );
			if ( right == null )
				throw new ArgumentNullException( "right" );

			_children.Add( null );
			_children.Add( null );

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
				InsertChild( value, 0 );
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
				InsertChild( value, 1 );
				Set( "Right", value );
			}
		}

		protected sealed override bool ValidatePropertiesCore()
		{
			return Left.ValidateProperties() && Right.ValidateProperties();
		}

		private void InsertChild( ExpressionBuilder child, int insertIndex )
		{
			while ( _children.Count <= insertIndex )
			{
				_children.Add( null );
			}
			_children[insertIndex] = child;
		}

		private readonly List<ExpressionBuilder> _children = new List<ExpressionBuilder>( 2 );
		[EditorBrowsable( EditorBrowsableState.Never )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Content )]
		public List<ExpressionBuilder> Children
		{
			get { return _children; }
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

		void ISupportInitialize.BeginInit() { }

		void ISupportInitialize.EndInit()
		{
			Left = Children[0];
			Right = Children[1];
		}

		#endregion
	}
}
