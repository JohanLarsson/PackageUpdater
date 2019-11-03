namespace PackageUpdater
{
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using System.Diagnostics;

    public partial class ChoresView : UserControl
    {
        public ChoresView()
        {
            this.InitializeComponent();
        }

        private void OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.LocalPath))?.Dispose();
            e.Handled = true;
        }
    }
}
