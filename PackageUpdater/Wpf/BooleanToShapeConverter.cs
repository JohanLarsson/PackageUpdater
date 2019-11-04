namespace PackageUpdater
{
    using System;
    using System.Windows.Data;
    using System.Windows.Markup;
    using System.Windows.Shapes;

    [ValueConversion(typeof(bool), typeof(Shape))]
    [MarkupExtensionReturnType(typeof(BooleanToShapeConverter))]
    public class BooleanToShapeConverter : MarkupExtension, IValueConverter
    {
        public Shape? WhenTrue { get; set; }
        
        public Shape? WhenFalse { get; set; }
        
        public Shape? WhenNull { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;

        public object? Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool b)
            {
                return b ? this.WhenTrue : this.WhenFalse;
            }

            return this.WhenNull;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException($"{nameof(BooleanToShapeConverter)} can only be used in OneWay bindings");
        }
    }
}