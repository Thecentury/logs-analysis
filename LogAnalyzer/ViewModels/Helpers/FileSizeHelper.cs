using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LogAnalyzer.GUI.ViewModels.Helpers
{
	public static class FileSizeHelper
	{
		private static readonly string[] names = new[] { "Bytes", "Kb", "Mb", "Gb" };

		public static string GetFormattedLength( long length )
		{
			double tempLength = length;
			int nameIndex = 0;
			while ( tempLength > 1024 )
			{
				tempLength /= 1024;
				nameIndex++;
			}

			double roundedLength = Math.Round( tempLength, 1 );
			string result = string.Format( "{0} {1}", roundedLength.ToString( CultureInfo.InvariantCulture ), names[nameIndex] );
			return result;
		}
	}
}
