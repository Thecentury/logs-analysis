using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using LogAnalyzer.Filters;
using LogAnalyzer.GUI.ViewModels.FilesTree;

namespace LogAnalyzer.GUI.ViewModels
{
	internal sealed class FilesTreeRequestShowVisitor : IFileTreeItemVisitor
	{
		private readonly ApplicationViewModel _application;

		public FilesTreeRequestShowVisitor( [NotNull] ApplicationViewModel application )
		{
			if ( application == null )
			{
				throw new ArgumentNullException( "application" );
			}
			_application = application;
		}

		public void ProcessRequestShow( object source )
		{
			IFileTreeVisitable visitable = source as IFileTreeVisitable;
			if ( visitable == null )
			{
				throw new ArgumentException( "Source is expected to implement IFileTreeVisitable interface." );
			}

			visitable.Accept( this );
		}

		public void Visit( FileTreeItem file )
		{
			var logFile = (LogFile)file.LogFile;

			var directoryViewModel = _application.CoreViewModel.Directories.First( d => d.LogDirectory == logFile.ParentDirectory );

			_application.Tabs.Add( new LogFileViewModel( logFile, directoryViewModel ) );
		}

		public void Visit( DirectoryTreeItem dir )
		{
			var filters = dir.Files.Where( f => f.IsChecked ).Select( f => new FileNameEquals( f.LogFile.Name ) ).Cast<ExpressionBuilder>().ToArray();
			OrCollection or = new OrCollection( filters );

			_application.Tabs.Add( new FilterTabViewModel( _application.Core.MergedEntries, _application, or ) );
		}
	}
}
