using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using JetBrains.Annotations;
using LogAnalyzer.Extensions;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.GUI.ViewModels.Helpers;

namespace LogAnalyzer.GUI.ViewModels.FilesTree
{
	public sealed class FileTreeItem : FileTreeItemBase, IRequestShow
	{
		private readonly ILogFile _logFile;

		public FileTreeItem( [NotNull] ILogFile logFile )
		{
			if ( logFile == null )
			{
				throw new ArgumentNullException( "logFile" );
			}
			this._logFile = logFile;
		}

		public override string Header
		{
			get { return LogFile.Name; }
		}

		public override string IconSource
		{
			get
			{
				string icon = FileNameToIconHelper.GetIcon( LogFile.Name );
				return PackUriHelper.MakePackUri( String.Format( "/Resources/{0}.png", icon ) );
			}
		}

		public override void Accept( IFileTreeItemVisitor visitor )
		{
			visitor.Visit( this );
		}

		private bool _isChecked;
		public bool IsChecked
		{
			get { return _isChecked; }
			set
			{
				_isChecked = value;
				RaisePropertyChanged( "IsChecked" );
			}
		}

		public event EventHandler<RequestShowEventArgs> RequestShow;

		// Commands

		// ShowCommand

		private DelegateCommand _showCommand;
		public ICommand ShowCommand
		{
			get
			{
				if ( _showCommand == null )
				{
					_showCommand = new DelegateCommand( ShowExecute );
				}

				return _showCommand;
			}
		}

		public ILogFile LogFile
		{
			get { return _logFile; }
		}

		private void ShowExecute()
		{
			RequestShow.Raise( this, new RequestShowEventArgs( this ) );
		}
	}
}
