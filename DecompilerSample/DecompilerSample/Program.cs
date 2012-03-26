using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using DecompilerSample.Namespace;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.ILSpy;
using Mono.Cecil;

namespace DecompilerSample
{
	class Hello
	{
		public Hello()
		{
			Logger logger = new Logger();
			logger.WriteMessage( "Test" );
		}
	}

	class Program
	{
		static void Main( string[] args )
		{
			Application app = new Application();

			var assembliesList = new AssemblyList( "1" );
			LoadedAssembly assembly = assembliesList.OpenAssembly(
				@"C:\Development\AWAD-Brinchuk\LogsAnalysis\DecompilerSample\DecompilerSample\bin\Debug\DecompilerSample.exe" );

			ModuleDefinition mainModule = assembly.AssemblyDefinition.MainModule;
			AstBuilder astBuilder = new AstBuilder( new DecompilerContext( mainModule )
			{
			} );
			astBuilder.AddAssembly( assembly.AssemblyDefinition );
			astBuilder.RunTransformations();

			Visitor visitor = new Visitor();
			astBuilder.CompilationUnit.AcceptVisitor( visitor, null );

			var children = astBuilder.CompilationUnit.Children;

			//StringBuilder builder = new StringBuilder();
			//astBuilder.GenerateCode( new PlainTextOutput( new StringWriter( builder ) ) );

			//Console.WriteLine( builder );

			Console.WriteLine( "End" );
			Console.ReadLine();

			//astBuilder.RunTransformations( this.transformAbortCondition );
			//astBuilder.GenerateCode( output );

			//CSharpLanguage lang = new CSharpLanguage();
			//var assemblyList = new AssemblyList( "list" );
			//var assembly = assemblyList.OpenAssembly( @"C:\Development\AWAD-Brinchuk\LogsAnalysis\DecompilerSample\DecompilerSample\bin\Debug\DecompilerSample.exe" );
			//StringBuilder builder = new StringBuilder();
			//var output = new PlainTextOutput( new StringWriter( builder ) );
			//lang.DecompileAssembly( assembly, output, new DecompilationOptions { } );

			//Console.WriteLine( builder.ToString() );
		}
	}
}

namespace DecompilerSample.Namespace
{
	public interface ILogger
	{
		void WriteMessage( string message );
	}

	public sealed class Logger : ILogger
	{
		public void WriteMessage( string message )
		{
			Console.WriteLine( message );
		}
	}
}