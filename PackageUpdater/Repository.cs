namespace PackageUpdater
{
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    public class Repository : INotifyPropertyChanged
    {
        public Repository(FileInfo sln, FileInfo dependencies, FileInfo lockFile, FileInfo paketExe)
        {
            this.Sln = sln;
            this.Dependencies = dependencies;
            this.LockFile = lockFile;
            this.PaketExe = paketExe;
            this.DeleteDotVsFolderCommand = new RelayCommand(
                _ => this.DeleteDotVs(),
                _ => this.DotVsDirectory != null);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public FileInfo Sln { get; }

        public FileInfo Dependencies { get; }

        public FileInfo LockFile { get; }

        public FileInfo PaketExe { get; }

        public DirectoryInfo RootDirectory => this.Sln.Directory;

        public ICommand DeleteDotVsFolderCommand { get; }

        public DirectoryInfo DotVsDirectory
        {
            get
            {
                if (this.RootDirectory.EnumerateDirectories(".vs").FirstOrDefault() is DirectoryInfo dir)
                {
                    return dir;
                }

                return null;
            }
        }

        public static bool TryCreate(string directory, out Repository repository)
        {
            if (Directory.EnumerateFiles(directory, "*.sln").FirstOrDefault() is string sln &&
                Directory.EnumerateFiles(directory, "paket.dependencies").FirstOrDefault() is string dependencies &&
                Directory.EnumerateFiles(directory, "paket.lock").FirstOrDefault() is string lockFile &&
                Directory.EnumerateDirectories(directory, ".paket").FirstOrDefault() is string paketDir &&
                Directory.EnumerateFiles(paketDir, "paket.exe").FirstOrDefault() is string paketExe)
            {
                repository = new Repository(new FileInfo(sln), new FileInfo(dependencies), new FileInfo(lockFile), new FileInfo(paketExe));
                return true;
            }

            repository = null;
            return false;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void DeleteDotVs()
        {
            try
            {
                this.DotVsDirectory?.Delete(true);
                this.OnPropertyChanged(nameof(this.DotVsDirectory));
            }
            catch
            {
                // just swallowing
            }
        }
    }
}