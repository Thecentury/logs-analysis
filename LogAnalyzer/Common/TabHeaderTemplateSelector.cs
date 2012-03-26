using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using AvalonDock.Layout;
using LogAnalyzer.GUI.ViewModels;
using LogAnalyzer.GUI.ViewModels.FilesDropping;

namespace LogAnalyzer.GUI.Common
{
	internal sealed class TabTemplateSelector : DataTemplateSelector
	{
		public DataTemplate LogEntriesListViewTemplate { get; set; }
		public DataTemplate LoadingTemplate { get; set; }
		public DataTemplate DropTemplate { get; set; }

		public override DataTemplate SelectTemplate( object item, DependencyObject container )
		{
			if ( item is LogEntriesListViewModel )
			{
				return LogEntriesListViewTemplate;
			}
			else if ( item is LoadingViewModel )
			{
				return LoadingTemplate;
			}
			else if ( item is DropFilesViewModel )
			{
				return DropTemplate;
			}
			else
			{
				return base.SelectTemplate(item, container);
			}
		}
	}

	internal sealed class TabHeaderTemplateSelector : DataTemplateSelector
	{
		public DataTemplate DefaultTemplate { get; set; }
		public DataTemplate FilterTabTemplate { get; set; }

		public override DataTemplate SelectTemplate( object item, DependencyObject container )
		{
			LayoutContent layoutContent = item as LayoutContent;

			if ( layoutContent == null )
			{
				return base.SelectTemplate( item, container );
			}
			else
			{
				var tab = layoutContent.Content;
				if ( tab is FilterTabViewModel )
				{
					return FilterTabTemplate;
				}
				else
				{
					return DefaultTemplate;
				}
			}
		}
	}
}
