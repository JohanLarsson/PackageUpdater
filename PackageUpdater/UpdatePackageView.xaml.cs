namespace PackageUpdater
{
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using System.Diagnostics;

    public partial class UpdatePackageView : UserControl
    {
        public UpdatePackageView()
        {
            this.InitializeComponent();
        }

        private void OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
