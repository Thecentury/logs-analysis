using System;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using LogAnalyzer.GUI.Common;

namespace LogAnalyzer.GUI.ViewModels.FilesDropping
{
	public sealed class LatestProject : BindingObject
	{
		private readonly string _path;
		private readonly DropFilesViewModel _parent;

		private DelegateCommand _openRecentProjectCommand;

		public LatestProject( [NotNull] string path, [NotNull] DropFilesViewModel parent )
		{
			if ( path == null )
			{
				throw new ArgumentNullException( "path" );
			}
			if ( parent == null )
			{
				throw new ArgumentNullException( "parent" );
			}

			_path = path;
			_parent = parent;
		}

		public ICommand OpenRecentProjectCommand
		{
			get
			{
				if ( _openRecentProjectCommand == null )
				{
					_openRecentProjectCommand = new DelegateCommand( () =>
					{
						Task dropCommandExecuteTask = _parent.DropCommandExecute( _path );
						dropCommandExecuteTask.ContinueWith( t => BeginInvokeInUIDispatcher( () => _parent.AnalyzeCommand.Execute() ) );
					} );
				}

				return _openRecentProjectCommand;
			}
		}

		public string Path
		{
			get { return _path; }
		}
	}
}