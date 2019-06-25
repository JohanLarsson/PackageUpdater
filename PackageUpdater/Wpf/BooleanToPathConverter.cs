namespace PackageUpdater
{
    using System;
    using System.Windows.Data;
    using System.Windows.Markup;
    using System.Windows.Media;
    using System.Windows.Shapes;

    [ValueConversion(typeof(bool), typeof(Geometry))]
    [MarkupExtensionReturnType(typeof(BooleanToPathConverter))]
    public class BooleanToPathConverter : MarkupExtension, IValueConverter
    {
        public Shape WhenTrue { get; set; }
        
        public Shape WhenFalse { get; set; }
        
        public Shape WhenNull { get; set; }

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