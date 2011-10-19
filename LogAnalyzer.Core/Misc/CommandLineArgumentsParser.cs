using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.Misc
{
	public sealed class CommandLineArgumentsParser
	{
		private readonly Dictionary<string, string> switchNameToValueMappings = new Dictionary<string, string>( StringComparer.InvariantCultureIgnoreCase );

		public CommandLineArgumentsParser( string[] commandLineArgs )
		{
			if ( commandLineArgs == null ) throw new ArgumentNullException( "commandLineArgs" );

			foreach ( var commandLineArg in commandLineArgs )
			{
				if ( commandLineArg.Contains( ":" ) )
				{
					string[] parts = commandLineArg.Split( ':' );
					string key = parts[0];
					string value = parts[1];
					switchNameToValueMappings.Add( key, value );
				}
			}
		}

		public bool ContainsSwitch( string key )
		{
			return switchNameToValueMappings.ContainsKey( key );
		}

		public string this[string key]
		{
			get { return switchNameToValueMappings[key]; }
		}

		public string GetValueOrDefault( string key, string defaultValue )
		{
			if ( ContainsSwitch( key ) )
				return this[key];
			else
				return defaultValue;
		}
	}
}
