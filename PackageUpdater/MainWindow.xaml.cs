namespace PackageUpdater
{
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
    }
}
