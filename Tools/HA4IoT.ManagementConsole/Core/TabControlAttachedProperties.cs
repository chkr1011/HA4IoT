using System.Windows;

namespace HA4IoT.ManagementConsole.Core
{
    public class TabControlAttachedProperties : FrameworkElement
    {
        public static readonly DependencyProperty VerticalHeadersProperty = DependencyProperty.RegisterAttached(
            "VerticalHeaders", typeof (bool), typeof (TabControlAttachedProperties), new PropertyMetadata(default(bool)));

        public static void SetVerticalHeaders(DependencyObject element, bool value)
        {
            element.SetValue(VerticalHeadersProperty, value);
        }

        public static bool GetVerticalHeaders(DependencyObject element)
        {
            return (bool) element.GetValue(VerticalHeadersProperty);
        }   
    }
}
