using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using LogAnalyzer;
using LogAnalyzer.Extensions;
using LogAnalyzer.GUI.ViewModel;
using AdTech.Common.WPF;
using LogAnalyzer.Filters;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace ExpressionBuilderSample
{
	internal class ExpressionBuilderViewModel : BindingObject
	{
		private readonly ParameterExpression parameter = null;

		protected ParameterExpression Parameter
		{
			get { return parameter; }
		}

		public ExpressionBuilderViewModel( ExpressionBuilder builder, ParameterExpression parameter )
		{
			if ( builder == null )
				throw new ArgumentNullException( "builder" );
			if ( parameter == null )
				throw new ArgumentNullException( "parameter" );

			this.builder = builder;
			this.parameter = parameter;

			// todo отписываться
			builder.PropertyChanged += OnBuilder_PropertyChanged;
		}

		private void OnBuilder_PropertyChanged( object sender, PropertyChangedEventArgs e )
		{
			RaiseAllPropertiesChanged();
		}

		protected virtual Type GetResultType()
		{
			Type result = builder.GetResultType( parameter );
			return result;
		}

		private List<ExpressionBuilderViewModel> builderViewModels = null;
		public IList<ExpressionBuilderViewModel> Builders
		{
			get
			{
				if ( builderViewModels == null )
				{
					Type resultType = GetResultType();
					ExpressionBuilder[] builders = ExpressionBuilderManager.GetBuildersReturningType( resultType );
					builderViewModels = builders.Select( b => ExpressionBuilderViewModelFactory.CreateViewModel( b, parameter ) ).ToList();
				}
				return builderViewModels;
			}
		}

		private readonly ExpressionBuilder builder = null;
		public ExpressionBuilder Builder
		{
			get { return builder; }
		}

		private ExpressionBuilderViewModel selectedChild = null;
		public ExpressionBuilderViewModel SelectedChild
		{
			get { return selectedChild; }
			set
			{
				selectedChild = value;
				RaisePropertyChanged( "SelectedChild" );
				RaisePropertyChanged( "IsInline" );
				RaisePropertyChanged( "Builder" );
				if ( selectedChild != null )
				{
					OnSelectedChildChanged( selectedChild.Builder );
				}
			}
		}

		protected virtual void OnSelectedChildChanged( ExpressionBuilder newBuilder )
		{
			TransparentBuilder transparentBuilder = builder as TransparentBuilder;
			if ( transparentBuilder != null )
			{
				transparentBuilder.Inner = newBuilder;
			}
		}

		public string Description
		{
			get
			{
				string description = builder.GetType().Name;
				return description;
			}
		}

		public virtual bool IsInline
		{
			get
			{
				bool isInline = false;
				if ( selectedChild != null )
				{
					ExpressionBuilder innerBuilder = selectedChild.Builder;
					if ( innerBuilder != null )
					{
						isInline = innerBuilder is IntermediateConstantBuilder;
					}
				}

				return isInline;
			}
		}
	}
}
