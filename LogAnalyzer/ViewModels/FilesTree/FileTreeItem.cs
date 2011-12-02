using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using JetBrains.Annotations;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.GUI.ViewModels.Helpers;

namespace LogAnalyzer.GUI.ViewModels.FilesTree
{
	public sealed class FileTreeItem : FileTreeItemBase
	{
		private readonly ILogFile logFile;

		public FileTreeItem( [NotNull] ILogFile logFile )
		{
			if ( logFile == null ) throw new ArgumentNullException( "logFile" );
			this.logFile = logFile;
		}

		public override string Header
		{
			get { return logFile.Name; }
		}


		public override string IconSource
		{
			get
			{
				string icon = FileNameToIconHelper.GetIcon( logFile.Name );
				return PackUriHelper.MakePackUri( String.Format( "/Resources/{0}.png", icon ) );
			}
		}

		private bool isChecked;
		public bool IsChecked
		{
			get { return isChecked; }
			set
			{
				isChecked = value;
				RaisePropertyChanged( "IsChecked" );
			}
		}
	}
}
