using System;

namespace LogAnalyzer.GUI.ViewModels.FilesTree
{
	public interface IRequestShow
	{
		event EventHandler<RequestShowEventArgs> RequestShow;
	}
}