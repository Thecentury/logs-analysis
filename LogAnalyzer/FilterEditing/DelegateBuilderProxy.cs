using System;
using LogAnalyzer.Filters;
using System.Linq.Expressions;
using LogAnalyzer.Extensions;
using System.Reflection;

namespace LogAnalyzer.GUI.FilterEditing
{
	[IgnoreBuilder]
	internal sealed class DelegateBuilderProxy : ExpressionBuilder
	{
		private readonly object _inner;
		private readonly PropertyInfo _propertyInfo;

		public DelegateBuilderProxy( object inner, string propertyName )
		{
			if ( inner == null )
			{
				throw new ArgumentNullException( "inner" );
			}
			if ( propertyName == null )
			{
				throw new ArgumentNullException( "propertyName" );
			}

			this._inner = inner;
			_propertyInfo = inner.GetType().GetProperty( propertyName, 
				BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty );
		}

		public ExpressionBuilder Inner
		{
			get { return (ExpressionBuilder)_propertyInfo.GetValue( _inner, null ); }
			set
			{
				_propertyInfo.SetValue( _inner, value, null );
				PropertyChangedDelegate.RaiseAllChanged( this );
			}
		}

		public override Type GetResultType( ParameterExpression target )
		{
			if ( Inner == null )
			{
				return typeof( object );
			}
			else
			{
				return Inner.GetResultType( target );
			}
		}

		protected override bool ValidatePropertiesCore()
		{
			return Inner != null;
		}

		protected override Expression CreateExpressionCore( ParameterExpression parameterExpression )
		{
			return Inner.CreateExpression( parameterExpression );
		}

		public Type GetPropertyType()
		{
			Type propertyType = null;

			object[] filterParameterAttributes = _propertyInfo.GetCustomAttributes( typeof( FilterParameterAttribute ), false );
			if ( filterParameterAttributes.Length > 0 )
			{
				FilterParameterAttribute filterParameterAttribute = (FilterParameterAttribute)filterParameterAttributes[0];
				propertyType = filterParameterAttribute.ParameterReturnType;
			}

			if ( propertyType == null )
			{
				IOverridePropertyTypeInfo obj = _inner as IOverridePropertyTypeInfo;
				if ( obj != null )
				{
					propertyType = obj.GetPropertyType( _propertyInfo.Name );
				}
			}

			if ( propertyType == null )
			{
				propertyType = typeof( object );
			}

			return propertyType;
		}
	}
}
