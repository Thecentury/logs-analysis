using System;

namespace LogAnalyzer.Filters
{
	[AttributeUsage( AttributeTargets.Class, AllowMultiple = false, Inherited = true )]
	public sealed class IconAttribute : Attribute
	{
		private readonly string _iconName;

		public IconAttribute(string iconName)
		{
			_iconName = iconName;
		}

		public string IconName
		{
			get { return _iconName; }
		}
	}
}