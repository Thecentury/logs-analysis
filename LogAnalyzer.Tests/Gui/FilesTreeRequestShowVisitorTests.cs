using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Concurrency;
using System.Text;
using LogAnalyzer.GUI.ViewModels;
using LogAnalyzer.GUI.ViewModels.FilesTree;
using NUnit.Framework;

namespace LogAnalyzer.Tests.Gui
{
	[TestFixture]
	public class FilesTreeRequestShowVisitorTests
	{
		[Test]
		public void ShouldCreateValidFilterForOneSelectedFileInOneDirectory()
		{
			CoreTreeItem coreTreeItem = CoreTreeItem.CreateEmpty();

			DirectoryTreeItem directoryTreeItem = new DirectoryTreeItem( LogDirectory.CreateEmpty( "123" ), Scheduler.Immediate );
			coreTreeItem.DirectoriesInternal.Add( directoryTreeItem );

			var fileTreeItem = new FileTreeItem( LogFile.CreateEmpty( "file" ) );
			directoryTreeItem.FilesInternal.Add( fileTreeItem );
			fileTreeItem.IsChecked = true;
			directoryTreeItem.UpdateIsChecked();

			var filter = FilesTreeRequestShowVisitor.CreateFilterForCore( coreTreeItem );
			bool isValid = filter.ValidateProperties();
			Assert.IsTrue( isValid );

			var expression = filter.CreateExpression( Expression.Parameter( typeof( LogEntry ) ) );
			Assert.IsNotNull( expression );
		}

		[Test]
		public void ShouldCreateValidFilterForOneSelectedFileInEachDirectory()
		{
			CoreTreeItem coreTreeItem = CoreTreeItem.CreateEmpty();

			DirectoryTreeItem directoryTreeItem1 = new DirectoryTreeItem( LogDirectory.CreateEmpty( "dir1" ), Scheduler.Immediate );
			coreTreeItem.DirectoriesInternal.Add( directoryTreeItem1 );
			DirectoryTreeItem directoryTreeItem2 = new DirectoryTreeItem( LogDirectory.CreateEmpty( "dir2" ), Scheduler.Immediate );
			coreTreeItem.DirectoriesInternal.Add( directoryTreeItem1 );

			var fileTreeItem1 = new FileTreeItem( LogFile.CreateEmpty( "file1" ) );
			directoryTreeItem1.FilesInternal.Add( fileTreeItem1 );
			fileTreeItem1.IsChecked = true;
			directoryTreeItem1.UpdateIsChecked();

			var fileTreeItem2 = new FileTreeItem( LogFile.CreateEmpty( "file1" ) );
			directoryTreeItem2.FilesInternal.Add( fileTreeItem1 );
			fileTreeItem2.IsChecked = true;
			directoryTreeItem2.UpdateIsChecked();

			var filter = FilesTreeRequestShowVisitor.CreateFilterForCore( coreTreeItem );
			bool isValid = filter.ValidateProperties();
			Assert.IsTrue( isValid );

			var expression = filter.CreateExpression( Expression.Parameter( typeof( LogEntry ) ) );
			Assert.IsNotNull( expression );
		}
	}
}
