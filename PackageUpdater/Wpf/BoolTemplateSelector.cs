namespace PackageUpdater
{
    using System.Windows;
    using System.Windows.Controls;

    public class BoolTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? WhenTrue { get; set; }

        public DataTemplate? WhenFalse { get; set; }
        
        public DataTemplate? WhenNull { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            if (item is bool b)
            {
                return b ? this.WhenTrue : this.WhenFalse;
            }

            return item is null ? this.WhenNull : base.SelectTemplate(item, container);
        }
    }
}
