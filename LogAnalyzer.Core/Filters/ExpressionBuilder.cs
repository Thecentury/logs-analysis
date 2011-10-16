using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.ComponentModel;
using LogAnalyzer.Extensions;
using System.Reflection;
using System.Diagnostics;
using System.Xaml;
using System.Xml;
using LogAnalyzer.Properties;

namespace LogAnalyzer.Filters
{
	public abstract class ExpressionBuilder : INotifyPropertyChanged
	{
		protected ExpressionBuilder()
		{
			RegisterAllSlots();
		}

		private void RegisterAllSlots()
		{
			var slotInfos = from property in GetType().GetProperties( BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy )
							let filterParameterAttributes = property.GetCustomAttributes( typeof( FilterParameterAttribute ), inherit: true )
							where filterParameterAttributes.Length > 0
							from FilterParameterAttribute attr in filterParameterAttributes
							select attr;

			foreach ( var slotInfo in slotInfos )
			{
				RegisterSlot( slotInfo.ParameterName, slotInfo.ParameterType );
			}
		}

		[DebuggerStepThrough]
		public Expression CreateExpression( ParameterExpression parameter )
		{
			if ( !ValidateProperties() )
			{
				throw new InvalidOperationException( "Properties validation failed." );
			}

			Expression result = CreateExpressionCore( parameter );

			return result;
		}

		[DebuggerStepThrough]
		public bool ValidateProperties()
		{
			Type myType = GetType();

			var propertiesToSet = from prop in myType.GetProperties( BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.FlattenHierarchy )
								  let attributes = prop.GetCustomAttributes( typeof( FilterParameterAttribute ), true )
								  where attributes.Length > 0
								  from FilterParameterAttribute attr in attributes
								  select attr.ParameterName;

			bool allPropertiesSet = propertiesToSet.All( HasValue );

			return allPropertiesSet;
		}

		public abstract Type GetResultType( ParameterExpression target );

		protected abstract Expression CreateExpressionCore( ParameterExpression parameterExpression );

		public event PropertyChangedEventHandler PropertyChanged;
		protected PropertyChangedEventHandler PropertyChangedDelegate
		{
			get { return PropertyChanged; }
		}

		#region Slots

		private readonly HashSet<Slot> slots = new HashSet<Slot>();
		private readonly Dictionary<Slot, object> slotValues = new Dictionary<Slot, object>();

		public IEnumerable<object> SlotValues
		{
			get { return slotValues.Values; }
		}

		protected bool HasValue( string slotName )
		{
			Slot slot = GetSlotByName( slotName );
			return slotValues.Any( s => s.Key == slot );
		}

		private Slot GetSlotByName( string slotName )
		{
			return slots.Single( s => s.Name == slotName );
		}

		private void RegisterSlot( string slotName, Type slotType )
		{
			Slot slot = new Slot( slotName, slotType );
			slots.Add( slot );
		}

		protected TProperty Get<TProperty>( string slotName )
		{
			Slot slot = slotValues.Keys.SingleOrDefault( s => s.Name == slotName );

			if ( slot == null )
				return default( TProperty );

			return (TProperty)slotValues[slot];
		}

		protected ExpressionBuilder GetExpressionBuilder( string slotName )
		{
			Slot slot = slotValues.Keys.SingleOrDefault( s => s.Name == slotName );
			if ( slot == null )
				return null;

			return (ExpressionBuilder)slotValues[slot];
		}

		protected void Set<TProperty>( string propertyName, TProperty value )
		{
			Slot slot = GetSlotByName( propertyName );

			if ( slotValues.ContainsKey( slot ) )
			{
				INotifyPropertyChanged oldBuilder = slotValues[slot] as INotifyPropertyChanged;
				if ( oldBuilder != null )
				{
					oldBuilder.PropertyChanged -= OnChildPropertyChanged;
				}
			}

			slotValues[slot] = value;

			INotifyPropertyChanged newBuilder = value as INotifyPropertyChanged;
			if ( newBuilder != null )
			{
				newBuilder.PropertyChanged += OnChildPropertyChanged;
			}

			PropertyChanged.Raise( this, propertyName );
		}

		private void OnChildPropertyChanged( object sender, PropertyChangedEventArgs e )
		{
			PropertyChanged.RaiseAllChanged( this );
		}

		#endregion

		#region Helpers

		protected ConstantExpression CreateConstantExpression<T>( T constant )
		{
			return Expression.Constant( constant, typeof( T ) );
		}

		protected MethodInfo GetMethod( LambdaExpression expression )
		{
			return ((MethodCallExpression)expression.Body).Method;
		}

		protected MethodInfo GetMethod<TResult>( Expression<Func<TResult>> expression )
		{
			return GetMethod( (LambdaExpression)expression );
		}

		protected MethodInfo GetMethod<T, TResult>( Expression<Func<T, TResult>> expression )
		{
			return GetMethod( (LambdaExpression)expression );
		}

		protected MethodInfo GetMethod<T1, T2, TResult>( Expression<Func<T1, T2, TResult>> expression )
		{
			return GetMethod( (LambdaExpression)expression );
		}

		#endregion

		public static ConstantExpressionBuilder<TConstant> CreateConstant<TConstant>( TConstant value )
		{
			return new ConstantExpressionBuilder<TConstant>( value );
		}

		public static ExpressionBuilder Parse( string str )
		{
			// приписываем xmlns, если это не было сделано раньше.
			if ( !str.Contains( "xmlns" ) )
			{
				int firstTagCloseIndex = str.IndexOf( '>' );
				if ( firstTagCloseIndex > -1 )
				{
					string prefix = str.Substring( 0, firstTagCloseIndex );
					string postfix = str.Substring( firstTagCloseIndex );

					str = prefix + " xmlns=\"" + GlobalConstants.XmlNamespace + "\"" + postfix;
				}
			}

			ExpressionBuilder builder = (ExpressionBuilder)XamlServices.Parse( str );
			return builder;
		}

		public static bool TryParse( string str, out ExpressionBuilder builder )
		{
			builder = null;

			if ( String.IsNullOrWhiteSpace( str ) )
				return false;

			try
			{
				builder = Parse( str );
				return true;
			}
			catch ( Exception exc )
			{
				if ( !(exc is XmlException || exc is XamlObjectWriterException) )
					throw;

				return false;
			}
		}

		public static bool CanParse( string xamlString )
		{
			if ( String.IsNullOrWhiteSpace( xamlString ) )
				return false;

			ExpressionBuilder builder = null;
			bool canParse = ExpressionBuilder.TryParse( xamlString, out builder );

			return canParse;
		}
	}
}
