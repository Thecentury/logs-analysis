using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace LogAnalyzer.GUI.ViewModels.FilesTree
{
	public sealed class CoreTreeItem : BindingObject
	{
		private readonly LogAnalyzerCore core;
		private readonly List<DirectoryTreeItem> directories;

		public CoreTreeItem( [NotNull] LogAnalyzerCore core )
		{
			if ( core == null ) throw new ArgumentNullException( "core" );
			this.core = core;
			this.directories = new List<DirectoryTreeItem>( core.Directories.Select( dir => new DirectoryTreeItem( dir ) ) );
		}

		public IList<DirectoryTreeItem> Directories
		{
			get { return directories; }
		}
	}
}