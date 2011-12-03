using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Xaml;
using JetBrains.Annotations;
using LogAnalyzer.Logging;

namespace LogAnalyzer.GUI.ViewModels.Colorizing
{
	public sealed class ColorizationLoader
	{
		private readonly string directoryPath;

		public ColorizationLoader( [NotNull] string directory )
		{
			if ( directory == null ) throw new ArgumentNullException( "directory" );
			if ( !Directory.Exists( directory ) )
				throw new InvalidOperationException( string.Format( "Directory '{0}' doesn't exist.", directory ) );

			this.directoryPath = directory;
		}

		public List<ColorizeTemplateBase> Load()
		{
			var files = Directory.GetFiles( directoryPath, "*.xaml", SearchOption.AllDirectories );

			List<ColorizeTemplateBase> templates = new List<ColorizeTemplateBase>( files.Length );

			foreach ( var file in files )
			{
				try
				{
					using ( var stream = File.OpenRead( file ) )
					{
						ColorizeTemplateBase template = (ColorizeTemplateBase)System.Windows.Markup.XamlReader.Load( stream );
						templates.Add( template );
					}
				}
				catch ( Exception exc )
				{
					Logger.Instance.WriteLine( MessageType.Error,
						string.Format( "Exception in ColorizationHelper.Load(): File = '{0}, Exception = {1} ", file, exc ) );
				}
			}

			return templates;
		}
	}
}
