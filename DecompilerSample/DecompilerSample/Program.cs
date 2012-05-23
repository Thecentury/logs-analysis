using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Decompiler.Core;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Ast;
using Mono.Cecil;

namespace DecompilerSample
{
	internal class Program
	{
		private static void Main( string[] args )
		{
			const string projectDir = @"C:\MOST\AWAD\Bin-server\Project\";
			const string platformDir = @"C:\MOST\AWAD\Bin-server\Common\";
			const string environmentDir = @"C:\MOST\AWAD\BIN-SERVER\ENVIROMENT";

			var dlls = Directory.GetFiles( projectDir, "*.dll" ).Concat( Directory.GetFiles( platformDir, "*.dll" ) );

			List<AssemblyDefinition> assemblies = dlls.AsParallel().Select( LoadAssembly ).ToList();

			DefaultAssemblyResolver resolver = (DefaultAssemblyResolver)GlobalAssemblyResolver.Instance;
			resolver.AddSearchDirectory( projectDir );
			resolver.AddSearchDirectory( platformDir );
			resolver.AddSearchDirectory( environmentDir );
			resolver.AddSearchDirectory( @"d:\Dev\Mikhail\Environment\ZLibs\" );
			resolver.AddSearchDirectory( @"d:\Dev\Mikhail\Environment\mongodb\" );
			resolver.AddSearchDirectory( @"d:\Dev\Mikhail\Environment\Microsoft\" );
			resolver.AddSearchDirectory( @"c:\MOST\AWAD\BIN-SERVER\PROJECT\Controllers\" );
			resolver.AddSearchDirectory( @"D:\Dev\Mikhail\Projects\Eticket\Version31_jet\Source\FastSerializer\bin\Debug" );

			List<LoggerUsageInAssembly> usages = new List<LoggerUsageInAssembly>();

			foreach ( var assembly in assemblies )
			{
				string name = assembly.Name.Name;
				if ( name.Contains( "XmlSerializers" ) )
				{
					Console.WriteLine( "Skipping '{0}'", name );
					continue;
				}

				Console.WriteLine( "Decompiling '{0}'", name );

				var module = assembly.MainModule;

				var decompilerContext = new DecompilerContext( module );
				AstBuilder astBuilder = new AstBuilder( decompilerContext );
				astBuilder.AddAssembly( assembly );

				var visitor = new Visitor();
				astBuilder.CompilationUnit.AcceptVisitor( visitor, decompilerContext );

				if ( visitor.Usages.Count > 0 )
				{
					LoggerUsageInAssembly usage = new LoggerUsageInAssembly( visitor.Usages ) { AssemblyName = assembly.Name.Name };
					usages.Add( usage );
				}

				Console.WriteLine( visitor.Usages.Count.ToString() );
			}

			using ( var stream = new FileStream( "usages.xml", FileMode.Create, FileAccess.Write ) )
			{
				LoggerUsageInAssembly.Serialize( usages, stream );
			}
		}

		private static AssemblyDefinition LoadAssembly( string dll )
		{
			bool readSymbols = File.Exists( Path.ChangeExtension( dll, "pdb" ) );
			var assembly = AssemblyDefinition.ReadAssembly( dll, new ReaderParameters( ReadingMode.Immediate ) { ReadSymbols = readSymbols } );

			Console.WriteLine( "Loaded '{0}'", dll );
			return assembly;
		}
	}
}