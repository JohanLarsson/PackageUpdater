namespace PackageUpdater
{
    using System;
    using System.Windows;
    using System.Windows.Controls;

    public class StatusTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Waiting { get; set; }
        public DataTemplate Running { get; set; }
        public DataTemplate Waiting { get; set; }
        public DataTemplate Waiting { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is Status status)
            {
                switch (status)
                {
                    case Status.Waiting:
                        return Waiting;
                        break;
                    case Status.Running:
                        break;
                    case Status.NoChange:
                        break;
                    case Status.Error:
                        break;
                    case Status.Success:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}