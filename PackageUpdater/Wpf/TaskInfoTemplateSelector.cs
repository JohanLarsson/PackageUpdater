namespace PackageUpdater
{
    using System.Windows;
    using System.Windows.Controls;

    public class TaskInfoTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Update { get; set; }

        public DataTemplate Replace { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return item switch
            {
                UpdatePackageChore _ => this.Update,
                ReplacePackageChore _ => this.Replace,
                _ => base.SelectTemplate(item, container),
            };
        }
    }
}
