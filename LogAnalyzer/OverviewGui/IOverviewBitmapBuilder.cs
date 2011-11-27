using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace LogAnalyzer.GUI.OverviewGui
{
	public interface IOverviewBitmapBuilder<in T>
	{
		BitmapSource CreateBitmap(T[] map);
	}
}
