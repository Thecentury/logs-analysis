using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Input;
using AdTech.Common.WPF;
using LogAnalyzer.Filters;
using LogAnalyzer.Extensions;

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

			foreach ( var child in builder.Children )
			{
				var childViewModel = ExpressionBuilderViewModelFactory.CreateViewModel( child, parameter );
				children.Add( childViewModel );
			}

			// дополняет до 2 детей.
			while ( children.Count < 2 )
			{
				var proxyViewModel = CreateProxyViewModel();
				children.Add( proxyViewModel );
			}
		}

		private ExpressionBuilderViewModel CreateProxyViewModel()
		{
			int index = children.Count;
			var proxy = new ProxyCollectionElementBuilder( children, index );
			var proxyViewModel = ExpressionBuilderViewModelFactory.CreateViewModel( proxy, parameter );
			return proxyViewModel;
		}

		private readonly ObservableCollection<ExpressionBuilderViewModel> children = new ObservableCollection<ExpressionBuilderViewModel>();

		public Collection<ExpressionBuilderViewModel> Children
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

		// todo brinchuk commands for removing
	}

	internal sealed class ProxyCollectionElementBuilder : ExpressionBuilder
	{
		private readonly IList<ExpressionBuilderViewModel> collection;
		public ProxyCollectionElementBuilder( IList<ExpressionBuilderViewModel> collection, int index )
		{
			if ( collection == null ) throw new ArgumentNullException( "collection" );

			this.collection = collection;
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
			get { return collection[index].Builder; }
			set
			{
				//collection[index].Builder = 
				PropertyChangedDelegate.RaiseAllChanged( this );
			}
		}
	}
}
