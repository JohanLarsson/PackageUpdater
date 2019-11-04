namespace PackageUpdater
{
    using System.Windows;
    using System.Windows.Data;

    [ValueConversion(typeof(string), typeof(Visibility))]
    public class StringToVisibilityCOnverter : IValueConverter
    {
        public static readonly StringToVisibilityCOnverter CollapsedWhenEmpty = new StringToVisibilityCOnverter(Visibility.Visible, Visibility.Collapsed);

        private readonly Visibility whenNotEmpty;
        private readonly Visibility whenEmpty;

        public StringToVisibilityCOnverter(Visibility whenNotEmpty, Visibility whenEmpty)
        {
            this.whenNotEmpty = whenNotEmpty;
            this.whenEmpty = whenEmpty;
        }
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return string.IsNullOrEmpty(value as string) ? this.whenEmpty : this.whenNotEmpty;
        }

        object IValueConverter.ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotSupportedException($"{nameof(StringToVisibilityCOnverter)} can only be used in OneWay bindings");
        }
    }
}
