using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LogAnalyzer.GUI.Common
{
	public interface ISaveToStreamDialog
	{
		Stream ShowDialog();
	}
}
