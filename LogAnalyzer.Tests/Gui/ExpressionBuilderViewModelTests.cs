using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using LogAnalyzer.Filters;
using LogAnalyzer.GUI.FilterEditing;
using NUnit.Framework;

namespace LogAnalyzer.Tests.Gui
{
	[TestFixture]
	public class ExpressionBuilderViewModelTests
	{
		[Test]
		public void ShouldIncludeGetDateByFileNameBuilder()
		{
			ParameterExpression parameter = Expression.Parameter( typeof( LogEntry ) );
			var vm = (BinaryBuilderViewModel)ExpressionBuilderViewModelFactory.CreateViewModel( new Equals(), parameter );

			var builders = vm.Left.GetAppropriateBuilders();
			bool containsGetDateByFileBuilder = builders.Any( b => b is GetDateByFileNameBuilder );
			Assert.That( containsGetDateByFileBuilder );
		}
	}
}
