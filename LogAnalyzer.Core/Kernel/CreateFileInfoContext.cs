﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer
{
	public sealed class CreateFileInfoContext
	{
		public bool UseCache { get; set; }
		public string FilePath { get; set; }
	}
}
