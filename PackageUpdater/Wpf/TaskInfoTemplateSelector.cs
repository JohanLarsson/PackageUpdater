﻿namespace PackageUpdater
{
    using System.Windows;
    using System.Windows.Controls;

    public class TaskInfoTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Update { get; set; }

        public DataTemplate Replace { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            switch (item)
            {
                case UpdatePackageInfo _:
                    return this.Update;
                case ReplacePackageInfo _:
                    return this.Replace;
                default:
                    return base.SelectTemplate(item, container);
            }
        }
    }
}