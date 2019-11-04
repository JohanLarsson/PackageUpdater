namespace PackageUpdater
{
    using System.Windows;
    using System.Windows.Data;

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public sealed class BooleanToVisibilityConverter : IValueConverter
    {
        public static readonly BooleanToVisibilityConverter CollapsedWhenFalse = new BooleanToVisibilityConverter(Visibility.Visible, Visibility.Collapsed, Visibility.Collapsed);

        private readonly object whenTrue;
        private readonly object whenFalse;
        private readonly object whenNull;

        public BooleanToVisibilityConverter(Visibility whenTrue, Visibility whenFalse, Visibility whenNull)
        {
            this.whenTrue = whenTrue;
            this.whenFalse = whenFalse;
            this.whenNull = whenNull;
        }

        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool b)
            {
                return b ? this.whenTrue : this.whenFalse;
            }

            return this.whenNull;
        }

        object IValueConverter.ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotSupportedException($"{nameof(BooleanToVisibilityConverter)} can only be used in OneWay bindings");
        }
    }
}