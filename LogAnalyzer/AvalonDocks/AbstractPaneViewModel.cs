using System;

namespace SampleApp.ViewModels
{
    /// <summary>
    /// Abstract base class for an AvalonDock pane view-model.
    /// </summary>
    public class AbstractPaneViewModel : AbstractViewModel
    {
        /// <summary>
        /// Set to 'true' when the pane is visible.
        /// </summary>
        private bool _isVisible = true;

        /// <summary>
        /// Set to 'true' when the pane is visible.
        /// </summary>
        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                if (_isVisible == value)
                {
                    return;
                }

                _isVisible = value;

                OnPropertyChanged("IsVisible");
            }
        }
    }
}
