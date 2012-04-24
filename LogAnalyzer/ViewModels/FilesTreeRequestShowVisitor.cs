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
			var filter = CreateFilterForDirectory( dir );

			_application.Tabs.Add( new FilterTabViewModel( _application.Core.MergedEntries, _application, filter ) );
		}

		public static ExpressionBuilder CreateFilterForDirectory( DirectoryTreeItem dir )
		{
			var filters =
				dir.Files.Where( f => f.IsChecked ).Select( f => new FileNameEquals( f.LogFile.Name ) ).Cast<ExpressionBuilder>().ToArray();
			var filter = ExpressionBuilder.CreateOr( filters );

			return filter;
		}

		public void Visit( CoreTreeItem core )
		{
			var filter = CreateFilterForCore( core );

			_application.Tabs.Add( new FilterTabViewModel( _application.Core.MergedEntries, _application, filter ) );
		}

		public static ExpressionBuilder CreateFilterForCore( CoreTreeItem core )
		{
			List<ExpressionBuilder> orParts = new List<ExpressionBuilder>();

			foreach ( var directory in core.Directories )
			{
				if ( directory.IsChecked == false )
				{
					continue;
				}

				List<ExpressionBuilder> fileFilters = new List<ExpressionBuilder>();
				foreach ( var file in directory.Files )
				{
					if ( file.IsChecked )
					{
						fileFilters.Add( new FileNameEquals( file.LogFile.Name ) );
					}
				}

				if ( fileFilters.Count > 0 )
				{
					And and = new And( new DirectoryNameEquals( directory.LogDirectory.DisplayName ),
									  ExpressionBuilder.CreateOr( fileFilters ) );
					orParts.Add( and );
				}
			}

			var or = ExpressionBuilder.CreateOr( orParts );
			return or;
		}
	}
}
