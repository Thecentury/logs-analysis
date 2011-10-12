using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.GUI.ViewModels;
using LogAnalyzer.Kernel;
using ModuleLogsProvider.Logging;

namespace ModuleLogsProvider.GUI.ViewModels
{
	public sealed class MostApplicationViewModel : ApplicationViewModel
	{
		public MostApplicationViewModel( MostLogAnalyzerConfiguration config, IEnvironment environment )
			: base( config, environment )
		{

		}
	}
}
