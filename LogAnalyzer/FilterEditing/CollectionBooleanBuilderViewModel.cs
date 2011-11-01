using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Input;
using LogAnalyzer.Filters;
using LogAnalyzer.Extensions;
using LogAnalyzer.GUI.Common;

namespace LogAnalyzer.GUI.FilterEditing
{
	internal sealed class CollectionBooleanBuilderViewModel : ExpressionBuilderViewModel
	{
		private readonly BooleanCollectionBuilder builder;
		private readonly ParameterExpression parameter;

		public CollectionBooleanBuilderViewModel( BooleanCollectionBuilder builder, ParameterExpression parameter )
			: base( builder, parameter )
		{
			if ( builder == null ) throw new ArgumentNullException( "builder" );
			if ( parameter == null ) throw new ArgumentNullException( "parameter" );

			this.builder = builder;
			this.parameter = parameter;

			int count = Math.Max( 2, builder.Children.Count );
			for ( int i = 0; i < count; i++ )
			{
				var proxyViewModel = CreateProxyViewModel();
				children.Add( proxyViewModel );
			}
		}

		private ExpressionBuilderViewModel CreateProxyViewModel()
		{
			int index = children.Count;
			var proxy = new ProxyCollectionElementBuilder( builder.Children, Children, index );
			var proxyViewModel = ExpressionBuilderViewModelFactory.CreateViewModel( proxy, parameter );

			if ( index < builder.Children.Count && builder.Children[index] != null )
			{
				var selectedViewModel = ExpressionBuilderViewModelFactory.CreateViewModel( builder.Children[index], parameter );
				proxyViewModel.SelectedChild = selectedViewModel;
			}

			return proxyViewModel;
		}

		private readonly ObservableCollection<ExpressionBuilderViewModel> children = new ObservableCollection<ExpressionBuilderViewModel>();
		public ObservableCollection<ExpressionBuilderViewModel> Children
		{
			get { return children; }
		}

		private DelegateCommand addChildCommand;
		public ICommand AddChildCommand
		{
			get
			{
				if ( addChildCommand == null )
					addChildCommand = new DelegateCommand( AddChildExecute );

				return addChildCommand;
			}
		}

		private void AddChildExecute()
		{
			var newChild = CreateProxyViewModel();
			children.Add( newChild );
		}
	}

	internal sealed class CollectionBooleanChildBuilderViewModel : ExpressionBuilderViewModel
	{
		private readonly ProxyCollectionElementBuilder builder;

		public CollectionBooleanChildBuilderViewModel( ProxyCollectionElementBuilder builder, ParameterExpression parameter )
			: base( builder, parameter )
		{
			if ( builder == null ) throw new ArgumentNullException( "builder" );

			this.builder = builder;
		}

		protected override void OnSelectedChildChanged( ExpressionBuilder newBuilder )
		{
			builder.Inner = newBuilder;
		}

		private DelegateCommand removeCommand;

		public ICommand RemoveCommand
		{
			get
			{
				if ( removeCommand == null )
					removeCommand = new DelegateCommand( ExecuteRemove );

				return removeCommand;
			}
		}

		private void ExecuteRemove()
		{
			if ( builder.Index < builder.Builders.Count )
			{
				builder.Builders.RemoveAt( builder.Index );
			}

			builder.ViewModels.RemoveAt( builder.Index );

			for ( int i = builder.Index; i < builder.ViewModels.Count; i++ )
			{
				var proxyBuilder = builder.ViewModels[i] as CollectionBooleanChildBuilderViewModel;
				if ( proxyBuilder != null )
				{
					proxyBuilder.builder.Index--;
				}
			}
		}
	}

	internal sealed class ProxyCollectionElementBuilder : ExpressionBuilder
	{
		private readonly IList<ExpressionBuilder> builders;
		public IList<ExpressionBuilder> Builders
		{
			get { return builders; }
		}

		private readonly IList<ExpressionBuilderViewModel> viewModels;
		public IList<ExpressionBuilderViewModel> ViewModels
		{
			get { return viewModels; }
		}

		public ProxyCollectionElementBuilder( IList<ExpressionBuilder> builders, IList<ExpressionBuilderViewModel> viewModels, int index )
		{
			if ( builders == null ) throw new ArgumentNullException( "builders" );
			if ( viewModels == null ) throw new ArgumentNullException( "viewModels" );

			this.builders = builders;
			this.viewModels = viewModels;
			this.index = index;
		}

		private int index;
		public int Index
		{
			get { return index; }
			set { index = value; }
		}

		protected override bool ValidatePropertiesCore()
		{
			return Inner != null;
		}

		public override Type GetResultType( ParameterExpression target )
		{
			return typeof( bool );
		}

		protected override Expression CreateExpressionCore( ParameterExpression parameterExpression )
		{
			return Inner.CreateExpression( parameterExpression );
		}

		public ExpressionBuilder Inner
		{
			get { return Builders[index]; }
			set
			{
				while ( Builders.Count <= index )
				{
					Builders.Add( null );
				}

				Builders[index] = value;
				PropertyChangedDelegate.RaiseAllChanged( this );
			}
		}
	}
}
