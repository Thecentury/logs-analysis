using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace LogAnalyzer.GUI.ViewModels.FilesDropping
{
	public sealed class StartAnalyzingVisitor : IDroppedObjectVisitor
	{
		private readonly LogDirectory droppedFilesDirectory;
		private readonly LogAnalyzerCore core;

		public StartAnalyzingVisitor( [NotNull] LogDirectory droppedFilesDirectory, [NotNull] LogAnalyzerCore core )
		{
			if ( droppedFilesDirectory == null ) throw new ArgumentNullException( "droppedFilesDirectory" );
			if ( core == null ) throw new ArgumentNullException( "core" );

			this.droppedFilesDirectory = droppedFilesDirectory;
			this.core = core;
		}

		public void Visit( DroppedFileViewModel file )
		{
			droppedFilesDirectory.Files.Add( file.LogFile );
		}

		public void Visit( DroppedDirectoryViewModel directory )
		{
			core.AddDirectory( directory.LogDirectory );
		}
	}
}
