namespace PackageUpdater
{
    using System;
    using System.ComponentModel;
    using System.Windows;

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                ((ViewModel) this.DataContext).GitDirectory = @"C:\Git";
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            Properties.Settings.Default.Save();
            base.OnClosed(e);
        }
    }
}
