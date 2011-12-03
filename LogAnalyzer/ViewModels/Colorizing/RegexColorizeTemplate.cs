using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace LogAnalyzer.GUI.ViewModels.Colorizing
{
	public sealed class RegexColorizeTemplate : FilteringColorizeTemplate
	{
		public string Pattern { get; set; }

		private Regex regex;
		private string[] groupNames;

		public override object GetDataContext( LogEntry logEntry )
		{
			string text = logEntry.UnitedText;
			IDictionary<string, object> context = new ExpandoObject();

			var match = regex.Match( text );
			foreach ( string groupName in groupNames )
			{
				string value = match.Groups[groupName].Value;

				context.Add( groupName, value );
			}

			context.Add( "Text", text );
			context.Add( "Template", Template );

			return context;
		}

		private readonly Regex getGroupNamesRegex = new Regex( @"\(\?<(?<Name>[a-zA-Z\d]+)>", RegexOptions.Compiled );

		public override void EndInit()
		{
			base.EndInit();

			if ( String.IsNullOrEmpty( Pattern ) )
				throw new ArgumentException( "Pattern should not be null or empty" );

			groupNames = getGroupNamesRegex.Matches( Pattern )
				.OfType<Match>()
				.SelectMany( m => m.Groups.OfType<Group>() )
				.Select( m => m.Value )
				.Where( name => !name.StartsWith( "(" ) )
				.ToArray();

			regex = new Regex( Pattern, RegexOptions.Compiled );
		}

		public override bool Accepts( LogEntry logEntry )
		{
			return base.Accepts( logEntry ) && regex.IsMatch( logEntry.UnitedText );
		}
	}
}
