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
                return status switch
                {
                    Status.Waiting => this.Waiting,
                    Status.Running => this.Running,
                    Status.NoChange => this.NoChange,
                    Status.Error => this.Error,
                    Status.Success => this.Success,
                    _ => throw new ArgumentOutOfRangeException(nameof(item)),
                };
            }

            return base.SelectTemplate(item, container);
        }
    }
}