using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.Decompiler;
using ICSharpCode.NRefactory.CSharp;
using Mono.Cecil;

namespace Decompiler.Core
{
	[Serializable]
	public sealed class LoggerUsage
	{
		public string ClassName { get; set; }
		public string MethodName { get; set; }
		public string FileName { get; set; }
		public int LineNumber { get; set; }
		public string MessageSeverity { get; set; }
		public string FormatString { get; set; }

		public static readonly int LineNotFound = -1;
	}

	[Serializable]
	public sealed class LoggerUsageInAssembly
	{
		private LoggerUsageInAssembly() { }

		public LoggerUsageInAssembly( List<LoggerUsage> usages )
		{
			this._usages = usages;
		}

		public string AssemblyName { get; set; }

		private readonly List<LoggerUsage> _usages;
		public List<LoggerUsage> Usages
		{
			get { return _usages; }
		}
	}

	public sealed class Visitor : DepthFirstAstVisitor<DecompilerContext, object>
	{
		private readonly List<LoggerUsage> _usages = new List<LoggerUsage>();

		public List<LoggerUsage> Usages
		{
			get { return _usages; }
		}

		public override object VisitInvocationExpression( InvocationExpression invocationExpression, DecompilerContext data )
		{
			MemberReferenceExpression expr = invocationExpression.Target as MemberReferenceExpression;
			if ( expr != null )
			{
				if ( expr.MemberName == "WriteLine" )
				{
					TypeReferenceExpression typeExpr = expr.Target as TypeReferenceExpression;
					if ( typeExpr != null )
					{
						SimpleType simpleType = typeExpr.Type as SimpleType;
						if ( simpleType != null )
						{
							if ( simpleType.Identifier == "LoggerExtension" )
							{
								var parentMethod = invocationExpression.Ancestors.OfType<AttributedNode>().First();
								var method = parentMethod.Annotation<MethodDefinition>();

								var args = invocationExpression.Arguments.ToList();

								int formatIndex = 2;
								if ( args.Count == 5 )
								{
									formatIndex = 3;
								}

								var formatString = Decompiler.Funcs.GetFormatString( args[formatIndex] );
								if ( formatString != null )
								{
									var messageSeverityArg = args[1] as MemberReferenceExpression;
									if ( messageSeverityArg != null )
									{

										var instruction = method.Body.Instructions.FirstOrDefault( i => i.OpCode.Name == "ldstr" && Equals( i.Operand, formatString ) );
										var instructionWithLocation = instruction.MoveBackwardFromSelf().FirstOrDefault( i => i.SequencePoint != null );
										if ( instructionWithLocation != null )
										{
											var location = instructionWithLocation.SequencePoint;

											_usages.Add( new LoggerUsage
															{
																ClassName = method.DeclaringType.FullName,
																MethodName = method.Name,
																FileName = location.Document.Url,
																LineNumber = location.StartLine,
																MessageSeverity = messageSeverityArg.MemberName,
																FormatString = formatString
															} );
										}
										else
										{
											_usages.Add( new LoggerUsage
											{
												ClassName = method.DeclaringType.FullName,
												MethodName = method.Name,
												FileName = null,
												LineNumber = LoggerUsage.LineNotFound,
												MessageSeverity = messageSeverityArg.MemberName,
												FormatString = formatString
											} );
										}
									}
									else
									{
									}
								}
							}
						}
					}
					else
					{

					}
				}
			}

			return base.VisitInvocationExpression( invocationExpression, data );
		}
	}
}