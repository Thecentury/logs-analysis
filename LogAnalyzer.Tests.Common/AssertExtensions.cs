using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace LogAnalyzer.Tests.Common
{
	public static class AssertExtensions
	{
		public static void AssertIsTrue( this bool condition )
		{
			Assert.IsTrue( condition );
		}

		public static void AssertIsFalse( this bool condition )
		{
			Assert.IsFalse( condition );
		}
	}
}
