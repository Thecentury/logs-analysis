using System;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.PatternMatching;
using Attribute = ICSharpCode.NRefactory.CSharp.Attribute;

namespace DecompilerSample
{
	public sealed class Visitor : DepthFirstAstVisitor<object, object>
	{
		public override object VisitInvocationExpression( InvocationExpression invocationExpression, object data )
		{
			return base.VisitInvocationExpression( invocationExpression, data );
		}
	}
}