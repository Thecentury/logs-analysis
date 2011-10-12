using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xaml;
using LogAnalyzer.Config;
using LogAnalyzer.Kernel;

namespace ModuleLogsProvider.Logging
{
	public class MostLogAnalyzerConfiguration : LogAnalyzerConfiguration
	{
		public static new MostLogAnalyzerConfiguration CreateNew()
		{
			return new MostLogAnalyzerConfiguration();
		}

		public ITimer LogsUpdateTimer { get; set; }

		public ITimer PerformanceDataUpdateTimer { get; set; }

		private readonly List<MostServerUrls> urls = new List<MostServerUrls>();
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Content )]
		public List<MostServerUrls> Urls
		{
			get { return urls; }
		}

		/// <summary>
		/// Адреса сервисов выбранного сервера.
		/// </summary>
		public MostServerUrls SelectedUrls { get; set; }

		public static new MostLogAnalyzerConfiguration LoadFromStream( Stream stream )
		{
			MostLogAnalyzerConfiguration config = (MostLogAnalyzerConfiguration)XamlServices.Load( stream );
			return config;
		}

		public static new MostLogAnalyzerConfiguration LoadFromFile( string fileName )
		{
			MostLogAnalyzerConfiguration result;

			using ( FileStream fs = new FileStream( fileName, FileMode.Open, FileAccess.Read, FileShare.Read ) )
			{
				result = LoadFromStream( fs );
			}

			return result;
		}
	}
}
