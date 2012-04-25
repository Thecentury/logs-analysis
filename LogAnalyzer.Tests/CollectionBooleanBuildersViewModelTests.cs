using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using LogAnalyzer.Filters;
using LogAnalyzer.GUI.FilterEditing;
using NUnit.Framework;

namespace LogAnalyzer.Tests
{
	[TestFixture]
	public class CollectionBooleanBuildersViewModelTests
	{
		[Test]
		public void TestRemoveCommand()
		{
			var parameter = Expression.Parameter( typeof( object ) );
			AndCollection and = new AndCollection();
			var vm = ExpressionBuilderViewModelFactory.CreateViewModel( new BuilderContext( and, parameter ) ) as CollectionBooleanBuilderViewModel;

			Assert.NotNull( vm );

			var child1 = vm.Children[0] as CollectionBooleanChildBuilderViewModel;
			Assert.NotNull( child1 );

			child1.RemoveCommand.Execute( null );

			Assert.AreEqual( 1, vm.Children.Count );

			var child2 = vm.Children[0] as CollectionBooleanChildBuilderViewModel;
			Assert.NotNull( child2 );

			child2.RemoveCommand.Execute( null );
			Assert.AreEqual( 0, vm.Children.Count );
		}
	}
}
