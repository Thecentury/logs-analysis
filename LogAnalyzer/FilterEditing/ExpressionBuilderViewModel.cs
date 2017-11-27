using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using JetBrains.Annotations;
using LogAnalyzer.Extensions;
using LogAnalyzer.Filters;
using System.Linq.Expressions;
using System.Reactive;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.GUI.ViewModels;

namespace LogAnalyzer.GUI.FilterEditing
{
	internal class ExpressionBuilderViewModel : BindingObject
	{
		private readonly ParameterExpression _parameter;
		private readonly BuilderContext _context;

		protected BuilderContext Context
		{
			get { return _context; }
		}

		protected ParameterExpression Parameter
		{
			get { return _parameter; }
		}

		public ExpressionBuilderViewModel( [NotNull] BuilderContext context )
		{
			if ( context == null )
			{
				throw new ArgumentNullException( "context" );
			}

			_builder = context.Builder;
			_parameter = context.Parameter;
			_context = context;

			_builder.ToNotifyPropertyChangedObservable()
				.SubscribeWeakly( this, OnBuilderPropertyChanged);
		}

	    private static void OnBuilderPropertyChanged(ExpressionBuilderViewModel vm,
	        EventPattern<PropertyChangedEventArgs> eventPattern)
	    {
	        vm.OnBuilderPropertyChanged(eventPattern.EventArgs);
	    }

        private void OnBuilderPropertyChanged( PropertyChangedEventArgs e )
		{
			RaiseAllPropertiesChanged();
		}

		protected virtual Type GetResultType()
		{
			Type result = _builder.GetResultType( _parameter );
			return result;
		}

		private List<ExpressionBuilderViewModel> _builderViewModels;
		public IList<ExpressionBuilderViewModel> Builders
		{
			get
			{
				if ( _builderViewModels == null )
				{
					var builders = GetAppropriateBuilders();
					_builderViewModels = builders.Select( b => ExpressionBuilderViewModelFactory.CreateViewModel( _context.WithBuilder( b ) ) ).ToList();

					ReplaceWithSelected();
				}

				return _builderViewModels;
			}
		}

		public ExpressionBuilder[] GetAppropriateBuilders()
		{
			Type resultType = GetResultType();
			ExpressionBuilder[] builders = ExpressionBuilderManager
				.GetBuilders( resultType, _context.InputType )
				.OrderBy( b => b.GetType().Name )
				.ToArray();

			return builders;
		}

		private void ReplaceWithSelected()
		{
			if ( _selectedChild != null )
			{
				var selectedBuilderType = SelectedChild.Builder.GetType();
				var builderWithTypeSameAsSelected = _builderViewModels.FirstOrDefault( b => b._builder.GetType() == selectedBuilderType );
				if ( builderWithTypeSameAsSelected != null )
				{
					int index = _builderViewModels.IndexOf( builderWithTypeSameAsSelected );
					_builderViewModels[index] = _selectedChild;
				}
			}
		}

		private readonly ExpressionBuilder _builder;
		public ExpressionBuilder Builder
		{
			get { return _builder; }
		}

		private ExpressionBuilderViewModel _selectedChild;
		public ExpressionBuilderViewModel SelectedChild
		{
			get { return _selectedChild; }
			set
			{
				_selectedChild = value;
				RaisePropertyChanged( "SelectedChild" );
				RaisePropertyChanged( "IsInline" );
				RaisePropertyChanged( "Builder" );
				if ( _selectedChild != null )
				{
					OnSelectedChildChanged( _selectedChild.Builder );
				}
			}
		}

		protected virtual void OnSelectedChildChanged( ExpressionBuilder newBuilder )
		{
			TransparentBuilder transparentBuilder = _builder as TransparentBuilder;
			if ( transparentBuilder != null )
			{
				transparentBuilder.Inner = newBuilder;
			}
		}

		public string Description
		{
			get
			{
				string description = _builder.ToString().Replace( "Filter", String.Empty ).Replace( "Builder", String.Empty );
				return description;
			}
		}

		public bool HasIcon
		{
			get
			{
				if ( !_iconLoaded )
				{
					LoadIcon();
				}

				return _icon != null;
			}
		}

		private bool _iconLoaded;
		private ImageSource _icon;
		public ImageSource Icon
		{
			get
			{
				if ( !_iconLoaded )
				{
					LoadIcon();
				}
				return _icon;
			}
		}

		private void LoadIcon()
		{
			var iconAttribute =
				_builder.GetType().GetCustomAttributes( typeof( IconAttribute ), true ).Cast<IconAttribute>().FirstOrDefault();

			if ( iconAttribute != null )
			{
				_icon = new BitmapImage( new Uri( PackUriHelper.MakePackUri( "/Resources/" + iconAttribute.IconName ) ) );
			}

			_iconLoaded = true;
		}

		public virtual bool IsInline
		{
			get
			{
				bool isInline = false;
				if ( _selectedChild != null )
				{
					ExpressionBuilder innerBuilder = _selectedChild.Builder;
					if ( innerBuilder != null )
					{
						isInline =
							innerBuilder is IntermediateConstantBuilder ||
							innerBuilder is DirectoryNameEqualsFilterBase ||
							innerBuilder is FileNameFilterBase;
					}
				}

				return isInline;
			}
		}
	}
}
