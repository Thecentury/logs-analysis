using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.GUI.ViewModels.FilesTree;
using LogAnalyzer.GUI.ViewModels.Helpers;
using Moq;
using NUnit.Framework;

namespace LogAnalyzer.Tests.Gui
{
	[TestFixture]
	public class TreeItemsTests
	{
		[TestCase( "2011-12-12-security", "security" )]
		[TestCase( "security", "security" )]
		public void TestIconSource( string name, string expectedShortName )
		{
			Mock<ILogFile> mock = new Mock<ILogFile>();
			mock.SetupGet( f => f.Name ).Returns( name );

			FileTreeItem item = new FileTreeItem( mock.Object );
			string icon = item.IconSource;

			string expectedIcon = FileNameToIconHelper.FileNameToIconMap[expectedShortName];

			Assert.IsTrue( icon.Contains( expectedIcon ) );
		}
	}
}
