namespace PackageUpdater
{
    using System;
    using System.Windows.Data;
    using System.Windows.Markup;
    using System.Windows.Media;

    [ValueConversion(typeof(bool), typeof(Geometry))]
    [MarkupExtensionReturnType(typeof(BooleanToBrushConverter))]
    public class BooleanToBrushConverter : MarkupExtension, IValueConverter
    {
        public Brush WhenTrue { get; set; }

        public Brush WhenFalse { get; set; }

        public Brush WhenNull { get; set; }

        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }

        object IValueConverter.ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool b)
            {
                return b ? this.WhenTrue : this.WhenFalse;
            }

            return this.WhenNull;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}