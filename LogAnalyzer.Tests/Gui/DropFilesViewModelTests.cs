using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using LogAnalyzer.Config;
using LogAnalyzer.GUI.Extensions;
using LogAnalyzer.GUI.ViewModels;
using LogAnalyzer.GUI.ViewModels.FilesDropping;
using LogAnalyzer.Kernel;
using LogAnalyzer.Tests.Common;
using LogAnalyzer.Tests.Mocks;
using Moq;
using NUnit.Framework;

namespace LogAnalyzer.Tests.Gui
{
	[TestFixture]
	public class DropFilesViewModelTests
	{
		[TestCase( 0, 0, 0, new object[] { "" } )]
		[TestCase( 1, 0, 1, new object[] { "file1" } )]
		[TestCase( 2, 0, 2, new object[] { "file1", "file2" } )]
		[TestCase( 1, 0, 3, new object[] { "file1", "nonExistingFile" } )]
		[TestCase( 1, 0, 4, new object[] { @"c:\Users" }, Description = "1 directory" )]
		public void ExecuteDropCommand( int totalFilesCount, int directoriesCount, int index, params object[] fileNames )
		{
			var fileSystemMock = CreateFileSystemMock();

			LogAnalyzerConfiguration config = new LogAnalyzerConfiguration()
				.RegisterInstance<IFileSystem>( fileSystemMock.Object )
				.RegisterInstance<IDirectoryFactory>( new DefaultDirectoryFactory() );

			var dropViewModel = CreateDropViewModel( config );

			ExpressionAssert.That( dropViewModel, d => d != null );

			ExpressionAssert.That( dropViewModel, d => d.DropCommand.CanExecute() );

			DataObject dataObject = new DataObject( "FileDrop", fileNames.Cast<string>().ToArray() );
			dropViewModel.DropCommand.Execute( dataObject );

			dropViewModel.Assert( dvm => dvm.Files.Count == totalFilesCount );
		}

		private static Mock<IFileSystem> CreateFileSystemMock()
		{
			Mock<IFileSystem> fileSystemMock = new Mock<IFileSystem>();
			fileSystemMock.Setup( fs => fs.FileExists( "file1" ) ).Returns( true );
			fileSystemMock.Setup( fs => fs.FileExists( "file2" ) ).Returns( true );
			fileSystemMock.Setup( fs => fs.DirectoryExists( "dir1" ) ).Returns( true );
			fileSystemMock.Setup( fs => fs.DirectoryExists( "dir2" ) ).Returns( true );
			fileSystemMock.Setup( fs => fs.DirectoryExists( @"c:\Users" ) ).Returns( true );
			return fileSystemMock;
		}

		[Test]
		public void ExecuteRemoveDroppedFileCommand()
		{
			LogAnalyzerConfiguration config = new LogAnalyzerConfiguration();
			var dropViewModel = CreateDropViewModel( config );

			var file = dropViewModel.AddDroppedFile( "SomePath" );
			ExpressionAssert.That( dropViewModel, d => d.Files.Count == 1 );

			file.RemoveFileCommand.Execute();

			ExpressionAssert.That( dropViewModel, d => d.Files.Count == 0 );
		}

		private static DropFilesViewModel CreateDropViewModel( LogAnalyzerConfiguration config )
		{
			MockEnvironment env = new MockEnvironment( config );

			ApplicationViewModel vm = new ApplicationViewModel( config, env );
			var dropViewModel = vm.Tabs.OfType<DropFilesViewModel>().First();
			return dropViewModel;
		}

		[Test]
		public void ExecuteClearCommand()
		{
			var dropViewModel = CreateDropViewModel();
			dropViewModel.ClearCommand.Execute();

			dropViewModel.Assert( d => d.Files.Count == 0 );

			dropViewModel.AddDroppedFile( "SomePath" );
			dropViewModel.Assert( d => d.Files.Count == 1 );

			dropViewModel.ClearCommand.Execute();
			dropViewModel.Assert( d => d.Files.Count == 0 );
		}

		private static DropFilesViewModel CreateDropViewModel()
		{
			var fileSystemMock = CreateFileSystemMock();
			var dropViewModel = CreateDropViewModel(
				new LogAnalyzerConfiguration()
				.RegisterInstance( fileSystemMock.Object )
				.RegisterInstance<IDirectoryFactory>( new DefaultDirectoryFactory() )
				);
			return dropViewModel;
		}

		[Test]
		public void CannotExecuteAnalyzeCommandOnEmptyFilesCollection()
		{
			var dropViewModel = CreateDropViewModel();

			dropViewModel.Assert( d => !d.AnalyzeCommand.CanExecute() );
		}

		[Test]
		public void ExecuteAnalyzeCommandWithFileAndDirectory()
		{
			var dropViewModel = CreateDropViewModel();
			dropViewModel.AddDroppedFile( "SomePath" );
			dropViewModel.AddDroppedDir( @"c:\Users" );

			dropViewModel.Assert( d => d.Files.Count == 2 );
			dropViewModel.Assert( d => d.AnalyzeCommand.CanExecute() );

			dropViewModel.AnalyzeCommand.Execute();

			Assert.AreEqual( 2, dropViewModel.ApplicationViewModel.Core.Directories.Count );
		}

		[Test]
		public void ExecuteAnalyzeCommandWithOneFile()
		{
			var dropVm = CreateDropViewModel();
			dropVm.AddDroppedFile( @"c:\Windows\notepad.exe" );

			dropVm.AnalyzeCommand.Execute();
			dropVm.Assert( d => d.ApplicationViewModel.Core.Directories.Count == 1 );
		}

		[Test]
		public void ExecuteAnalyzeCommandWithOneDirectory()
		{
			var dropVm = CreateDropViewModel();
			dropVm.AddDroppedDir( @"c:\Users" );

			dropVm.AnalyzeCommand.Execute();
			dropVm.Assert( d => d.ApplicationViewModel.Core.Directories.Count == 1 );
		}
	}
}
