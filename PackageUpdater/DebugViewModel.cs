namespace PackageUpdater
{
    using System.IO;
    using System.Windows.Input;
    using PackageUpdater.Tasks;

    public class DebugViewModel
    {
        public DebugViewModel()
        {
            this.RestoreCommand = new RelayCommand(_ => this.RestoreTask.Start());
        }

        public RestoreTask RestoreTask { get; } = new RestoreTask(new DirectoryInfo("C:\\Git\\_GuOrg\\Gu.Inject"));

        public ICommand RestoreCommand { get; }
    }
}