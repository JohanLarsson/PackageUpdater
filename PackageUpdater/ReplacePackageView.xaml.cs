namespace PackageUpdater
{
    using System.Diagnostics;
    using System.Windows.Controls;
    using System.Windows.Navigation;

    public partial class ReplacePackage : UserControl
    {
        public ReplacePackage()
        {
            this.InitializeComponent();
        }

        private void OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri))?.Dispose();
            e.Handled = true;
        }
    }
}
