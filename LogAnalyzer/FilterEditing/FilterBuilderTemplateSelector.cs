using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using LogAnalyzer.Filters;

namespace LogAnalyzer.GUI.FilterEditing
{
	public sealed class FilterBuilderTemplateSelector : DataTemplateSelector
	{
		private static readonly Dictionary<Type, object> templateMappings = new Dictionary<Type, object>();
		private static readonly TypesHierarchyComparer comparer = new TypesHierarchyComparer();

		static FilterBuilderTemplateSelector()
		{
			Register<BooleanCollectionBuilder>();

			Register<LogDateTimeFilterBase>();

			Register<Not>();
			Register<StaticBuilder>();
			Register<BinaryExpressionBuilder>();

			Register<StringConstant>( "ConstantTemplate" );
			Register<IntConstant>( "ConstantTemplate" );
			Register<DoubleConstant>( "ConstantTemplate" );
			Register<BoolConstant>( "ConstantTemplate" );
			Register<DateTimeConstant>( "ConstantTemplate" );
			Register<TimeSpanConstant>( "ConstantTemplate" );

			Register<StringFilterBuilder>();
			Register<ThreadIdEquals>();
			Register<TextContains>();
			Register<GetProperty>();
			Register<FileNameEquals>();
			Register<MessageTypeEquals>();
		}

		private static void Register<T>()
		{
			templateMappings.Add( typeof( T ), typeof( T ) );
		}

		private static void Register<T>( string resourceKey )
		{
			templateMappings.Add( typeof( T ), resourceKey );
		}

		private static object GetResourceKey( Type builderType )
		{
			Type key = templateMappings.Keys.Where( t => t.IsAssignableFrom( builderType ) ).OrderBy( comparer ).First();
			return templateMappings[key];
		}

		public override DataTemplate SelectTemplate( object item, DependencyObject container )
		{
			if ( item == null )
			{
				return base.SelectTemplate( item, container );
			}

			ExpressionBuilderViewModel vm = (ExpressionBuilderViewModel)item;
			ExpressionBuilder builder = vm.Builder;
			Type builderType = builder.GetType();

			FrameworkElement visual = (FrameworkElement)container;
			object resourceKey = GetResourceKey( builderType );
			DataTemplate template = (DataTemplate)visual.FindResource( resourceKey );
			return template;
		}

		private sealed class TypesHierarchyComparer : IComparer<Type>
		{
			public int Compare( Type x, Type y )
			{
				if ( x == y )
					return 0;

				if ( x.IsSubclassOf( y ) )
					return 1; // x > y
				else
					return -1;
			}
		}

	}
}
