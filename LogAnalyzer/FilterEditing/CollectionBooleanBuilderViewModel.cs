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
		private readonly BooleanCollectionBuilder _builder;

		public CollectionBooleanBuilderViewModel( BuilderContext<BooleanCollectionBuilder> context )
			: base( context )
		{
			this._builder = context.TypedBuilder;

			int count = Math.Max( 2, _builder.Children.Count );
			for ( int i = 0; i < count; i++ )
			{
				var proxyViewModel = CreateProxyViewModel();
				_children.Add( proxyViewModel );
			}
		}

		private ExpressionBuilderViewModel CreateProxyViewModel()
		{
			int index = _children.Count;
			var proxy = new ProxyCollectionElementBuilder( _builder.Children, Children, index );
			var proxyViewModel = ExpressionBuilderViewModelFactory.CreateViewModel( Context.WithBuilder( proxy ) );

			if ( index < _builder.Children.Count && _builder.Children[index] != null )
			{
				var selectedViewModel = ExpressionBuilderViewModelFactory.CreateViewModel( Context.WithBuilder( _builder.Children[index] ) );
				proxyViewModel.SelectedChild = selectedViewModel;
			}

			return proxyViewModel;
		}

		private readonly ObservableCollection<ExpressionBuilderViewModel> _children = new ObservableCollection<ExpressionBuilderViewModel>();
		public ObservableCollection<ExpressionBuilderViewModel> Children
		{
			get { return _children; }
		}

		private DelegateCommand _addChildCommand;
		public ICommand AddChildCommand
		{
			get
			{
				if ( _addChildCommand == null )
					_addChildCommand = new DelegateCommand( AddChildExecute );

				return _addChildCommand;
			}
		}

		private void AddChildExecute()
		{
			var newChild = CreateProxyViewModel();
			_children.Add( newChild );
		}
	}

	internal sealed class CollectionBooleanChildBuilderViewModel : ExpressionBuilderViewModel
	{
		private readonly ProxyCollectionElementBuilder _builder;

		public CollectionBooleanChildBuilderViewModel( BuilderContext<ProxyCollectionElementBuilder> context )
			: base( context )
		{
			_builder = context.TypedBuilder;
		}

		protected override void OnSelectedChildChanged( ExpressionBuilder newBuilder )
		{
			_builder.Inner = newBuilder;
		}

		private DelegateCommand _removeCommand;

		public ICommand RemoveCommand
		{
			get
			{
				if ( _removeCommand == null )
					_removeCommand = new DelegateCommand( ExecuteRemove );

				return _removeCommand;
			}
		}

		private void ExecuteRemove()
		{
			if ( _builder.Index < _builder.Builders.Count )
			{
				_builder.Builders.RemoveAt( _builder.Index );
			}

			_builder.ViewModels.RemoveAt( _builder.Index );

			for ( int i = _builder.Index; i < _builder.ViewModels.Count; i++ )
			{
				var proxyBuilder = _builder.ViewModels[i] as CollectionBooleanChildBuilderViewModel;
				if ( proxyBuilder != null )
				{
					proxyBuilder._builder.Index--;
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
