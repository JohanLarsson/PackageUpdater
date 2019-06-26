namespace PackageUpdater
{
    using System;
    using System.Windows;
    using System.Windows.Controls;

    public static class ScrollViewer
    {
        public static readonly DependencyProperty AutoScrollToBottomProperty = DependencyProperty.RegisterAttached(
            "AutoScrollToBottom",
            typeof(bool),
            typeof(ScrollViewer),
            new PropertyMetadata(
                false,
                OnAutoScrollToBottomChanged));

        public static bool GetAutoScrollToBottom(System.Windows.Controls.ScrollViewer element) => (bool)element.GetValue(AutoScrollToBottomProperty);

        public static void SetAutoScrollToBottom(System.Windows.Controls.ScrollViewer element, bool value) => element.SetValue(AutoScrollToBottomProperty, value);

        private static void OnAutoScrollToBottomChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is System.Windows.Controls.ScrollViewer scrollViewer &&
                e.NewValue is bool value)
            {
                if (value)
                {
                    scrollViewer.AutoScrollToBottom();
                    scrollViewer.ScrollChanged += ScrollChanged;
                }
                else
                {
                    scrollViewer.ScrollChanged -= ScrollChanged;
                }
            }
        }

        private static void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender is System.Windows.Controls.ScrollViewer scrollViewer &&
                e.ExtentHeightChange > 0)
            {
                scrollViewer.AutoScrollToBottom();
            }
        }

        private static void AutoScrollToBottom(this System.Windows.Controls.ScrollViewer scrollViewer)
        {
            if (Math.Abs(scrollViewer.VerticalOffset + scrollViewer.ActualHeight - scrollViewer.ExtentHeight) < 1)
            {
                scrollViewer.ScrollToBottom();
            }
        }
    }
}