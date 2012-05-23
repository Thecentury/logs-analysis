using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace LogAnalyzer.GUI.ViewModels.Colorizing
{
	public sealed class ColorizationManager
	{
		private readonly List<ColorizeTemplateBase> templates;

		public ColorizationManager( [NotNull] List<ColorizeTemplateBase> templates )
		{
			if ( templates == null )
			{
				throw new ArgumentNullException( "templates" );
			}
			this.templates = templates;
		}

		public ColorizeTemplateBase GetTemplateForEntry( LogEntry logEntry )
		{
			var template = templates
				.Where( t => t.Accepts( logEntry ) )
				.OrderByDescending( t => t.Priority )
				.First();

			return template;
		}
	}
}
