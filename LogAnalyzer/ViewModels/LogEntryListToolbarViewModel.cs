using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using JetBrains.Annotations;
using LogAnalyzer.Extensions;

namespace LogAnalyzer.GUI.ViewModels
{
	[IgnoreAllMissingProperties]
	internal sealed class LogEntryListToolbarViewModel : BindingObject
	{
		private readonly LogEntriesListViewModel _parent;

		public LogEntryListToolbarViewModel( [NotNull] LogEntriesListViewModel parent )
			: base( parent )
		{
			if ( parent == null ) throw new ArgumentNullException( "parent" );
			this._parent = parent;
		}

		public ICommand ScrollToBottomCommand
		{
			get { return _parent.ScrollToBottomCommand; }
		}

		public ICommand ScrollToTopCommand
		{
			get { return _parent.ScrollToTopCommand; }
		}

		public bool AutoScrollToBottom
		{
			get { return _parent.AutoScrollToBottom; }
			set { _parent.AutoScrollToBottom = value; }
		}
	}

	public sealed class ToggleButtonViewModel : BindingObject
	{
		private readonly Func<bool> _getIsToggledFunc;
		private readonly Action<bool> _setIsToggledFunc;

		public ToggleButtonViewModel( [NotNull] Func<bool> getIsToggledFunc, [NotNull] Action<bool> setIsToggledFunc,
			[NotNull] string tooltip, [NotNull] string iconSource )
		{
			if ( getIsToggledFunc == null )
			{
				throw new ArgumentNullException( "getIsToggledFunc" );
			}
			if ( setIsToggledFunc == null )
			{
				throw new ArgumentNullException( "setIsToggledFunc" );
			}
			if ( tooltip == null )
			{
				throw new ArgumentNullException( "tooltip" );
			}
			if ( iconSource == null )
			{
				throw new ArgumentNullException( "iconSource" );
			}

			_getIsToggledFunc = getIsToggledFunc;
			_setIsToggledFunc = setIsToggledFunc;

			Tooltip = tooltip;
			IconSource = iconSource;
		}

		public bool IsToggled
		{
			get
			{
				bool isToggled = _getIsToggledFunc();
				return isToggled;
			}
			set { _setIsToggledFunc( value ); }
		}

		public void RaiseIsToggledChanged()
		{
			RaisePropertyChanged( "IsToggled" );
		}

		public string Tooltip { get; set; }

		public string IconSource { get; set; }
	}
}
