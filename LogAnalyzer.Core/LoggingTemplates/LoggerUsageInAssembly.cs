using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace LogAnalyzer.LoggingTemplates
{
	[Serializable]
	public sealed class LoggerUsageInAssembly
	{
		public LoggerUsageInAssembly() { }

		public LoggerUsageInAssembly( List<LoggerUsage> usages )
		{
			this._usages = usages;
		}

		public string AssemblyName { get; set; }

		private readonly List<LoggerUsage> _usages = new List<LoggerUsage>();
		[XmlArray( "Usages" )]
		[XmlArrayItem( "LoggerUsage" )]
		public List<LoggerUsage> Usages
		{
			get { return _usages; }
		}

		public static void Serialize( List<LoggerUsageInAssembly> usages, Stream stream )
		{
			XmlSerializer serializer = new XmlSerializer( usages.GetType(), "" );
			XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
			namespaces.Add( "", "" );
			serializer.Serialize( stream, usages );
		}

		public static List<LoggerUsageInAssembly> Deserialize( Stream stream )
		{
			XmlSerializer serializer = new XmlSerializer( typeof( List<LoggerUsageInAssembly> ), "" );
			XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
			namespaces.Add( "", "" );
			var usages = (List<LoggerUsageInAssembly>)serializer.Deserialize( stream );

			return usages;
		}
	}
}
