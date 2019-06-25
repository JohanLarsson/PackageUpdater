namespace PackageUpdater
{
    using System;
    using System.Windows;
    using System.Windows.Controls;

    public class StatusTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Waiting { get; set; }
       
        public DataTemplate Running { get; set; }
        
        public DataTemplate NoChange { get; set; }
        
        public DataTemplate Error { get; set; }
        
        public DataTemplate Success { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is Status status)
            {
                switch (status)
                {
                    case Status.Waiting:
                        return this.Waiting;
                    case Status.Running:
                        return this.Running;
                    case Status.NoChange:
                        return this.NoChange;
                    case Status.Error:
                        return this.Error;
                    case Status.Success:
                        return this.Success;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return base.SelectTemplate(item, container);
        }
    }
}