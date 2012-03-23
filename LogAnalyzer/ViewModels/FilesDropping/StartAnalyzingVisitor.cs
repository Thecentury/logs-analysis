using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace LogAnalyzer.GUI.ViewModels.FilesDropping
{
	public sealed class StartAnalyzingVisitor : IDroppedObjectVisitor
	{
		private readonly LogAnalyzerCore _core;

		public StartAnalyzingVisitor([NotNull] LogAnalyzerCore core )
		{
			if ( core == null ) throw new ArgumentNullException( "core" );

			this._core = core;
		}

		public void Visit( DroppedFileViewModel file )
		{
			//_droppedFilesDirectory.Files.Add( file.LogFile );
		}

		public void Visit( DroppedDirectoryViewModel directory )
		{
			_core.AddDirectory( directory.LogDirectory );
		}
	}
}
