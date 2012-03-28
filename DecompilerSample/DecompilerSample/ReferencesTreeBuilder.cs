using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace DecompilerSample
{
	public static class ReferencesTreeBuilder
	{
		public static ReferencesTreeItem<AssemblyDefinition> BuildAssembliesTree( ModuleDefinition module, List<AssemblyDefinition> loadedAssemblies )
		{
			ReferencesTreeItem<AssemblyDefinition> tree = new ReferencesTreeItem<AssemblyDefinition>( module.Assembly );

			LoadChildren( module, loadedAssemblies, tree );

			return tree;
		}

		private static void LoadChildren( ModuleDefinition module, List<AssemblyDefinition> loadedAssemblies, ReferencesTreeItem<AssemblyDefinition> tree )
		{
			foreach ( var reference in module.AssemblyReferences )
			{
				AssemblyDefinition assembly = loadedAssemblies.FirstOrDefault( a => a.FullName == reference.FullName );
				if ( assembly != null )
				{
					var node = tree.AddChild( assembly );
					LoadChildren( assembly.MainModule, loadedAssemblies, node );
				}
			}
		}
	}
}