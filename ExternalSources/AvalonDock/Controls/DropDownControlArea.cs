﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Input;
using System.Diagnostics;

namespace AvalonDock.Controls
{
    public class DropDownControlArea : FrameworkElement
    {
        static DropDownControlArea()
        {
            IsHitTestVisibleProperty.OverrideMetadata(typeof(DropDownControlArea), new FrameworkPropertyMetadata(true));
        }
        
        public DropDownControlArea()
        { 
        
        }

        #region DropDownContextMenu

        /// <summary>
        /// DropDownContextMenu Dependency Property
        /// </summary>
        public static readonly DependencyProperty DropDownContextMenuProperty =
            DependencyProperty.Register("DropDownContextMenu", typeof(ContextMenu), typeof(DropDownControlArea),
                new FrameworkPropertyMetadata((ContextMenu)null));

        /// <summary>
        /// Gets or sets the DropDownContextMenu property.  This dependency property 
        /// indicates context menu to show when a right click is detected over the area occpied by the control.
        /// </summary>
        public ContextMenu DropDownContextMenu
        {
            get { return (ContextMenu)GetValue(DropDownContextMenuProperty); }
            set { SetValue(DropDownContextMenuProperty, value); }
        }

        #endregion

        #region DropDownContextMenuDataContext

        /// <summary>
        /// DropDownContextMenuDataContext Dependency Property
        /// </summary>
        public static readonly DependencyProperty DropDownContextMenuDataContextProperty =
            DependencyProperty.Register("DropDownContextMenuDataContext", typeof(object), typeof(DropDownControlArea),
                new FrameworkPropertyMetadata((object)null));

        /// <summary>
        /// Gets or sets the DropDownContextMenuDataContext property.  This dependency property 
        /// indicates data context to attach when context menu is shown.
        /// </summary>
        public object DropDownContextMenuDataContext
        {
            get { return (object)GetValue(DropDownContextMenuDataContextProperty); }
            set { SetValue(DropDownContextMenuDataContextProperty, value); }
        }

        #endregion

        protected override void OnMouseRightButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);

            if (!e.Handled)
            {
                if (DropDownContextMenu != null)
                {
                    DropDownContextMenu.Placement = PlacementMode.MousePoint;
                    DropDownContextMenu.DataContext = DropDownContextMenuDataContext;
                    DropDownContextMenu.IsOpen = true;
                }
            }
        }

        protected override void OnPreviewMouseRightButtonUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnPreviewMouseRightButtonUp(e);

        }


        protected override System.Windows.Media.HitTestResult HitTestCore(System.Windows.Media.PointHitTestParameters hitTestParameters)
        {
            return new PointHitTestResult(this, hitTestParameters.HitPoint);
        }
    }
}
