using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace LogAnalyzer.Tests
{
	public sealed class TotalOperationsCountHelper
	{
		private readonly LogAnalyzerCore core = null;
		private int operationsCount = 0;

		public TotalOperationsCountHelper( LogAnalyzerCore core )
		{
			if ( core == null )
				throw new ArgumentNullException( "core" );

			this.core = core;
			operationsCount = core.OperationsQueue.TotalOperationsCount;
		}

		public void AssertOperationsIncreased()
		{
			int currentOperationsCount = core.OperationsQueue.TotalOperationsCount;

			Assert.IsTrue( currentOperationsCount > operationsCount, "LogAnalyzer's total operation count haven't increased." );

			operationsCount = currentOperationsCount;
		}
	}
}
