namespace PackageUpdater
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;

    public sealed class Repository : INotifyPropertyChanged, System.IDisposable
    {
        private bool disposed;

        private Repository(DirectoryInfo gitDirectory, FileInfo dependencies, FileInfo lockFile, FileInfo paketExe)
        {
            this.GitDirectory = gitDirectory;
            this.Dependencies = dependencies;
            this.LockFile = lockFile;
            this.PaketExe = paketExe;
            this.SolutionFiles = new ReadOnlyObservableCollection<FileInfo>(new ObservableCollection<FileInfo>(gitDirectory.EnumerateFiles("*.sln", SearchOption.TopDirectoryOnly)));
            this.DotnetRestore = new DotnetRestore(this.GitDirectory);
            this.EmptyDiff = new GitAssertEmptyDiff(this.GitDirectory);
            this.IsOnMaster = new GitAssertIsOnMaster(this.GitDirectory);
            InitializeAsync();
            async void InitializeAsync()
            {
                try
                {
                    await this.EmptyDiff.RunAsync().ConfigureAwait(false);
                    await this.IsOnMaster.RunAsync().ConfigureAwait(false);
                }
                catch
                {
                    Debugger.Break();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public DirectoryInfo GitDirectory { get; }

        public ReadOnlyObservableCollection<FileInfo> SolutionFiles { get; }

        public FileInfo Dependencies { get; }

        public FileInfo LockFile { get; }

        public FileInfo PaketExe { get; }

        public DotnetRestore DotnetRestore { get; }

        public GitAssertEmptyDiff EmptyDiff { get; }

        public GitAssertIsOnMaster IsOnMaster { get; }

        public static bool TryCreate(string directory, out Repository repository)
        {
            if (Directory.EnumerateFiles(directory, "paket.dependencies").FirstOrDefault() is string dependencies &&
                Directory.EnumerateFiles(directory, "paket.lock").FirstOrDefault() is string lockFile &&
                Directory.EnumerateDirectories(directory, ".paket").FirstOrDefault() is string paketDir &&
                Directory.EnumerateFiles(paketDir, "paket.exe").FirstOrDefault() is string paketExe)
            {
                repository = new Repository(new DirectoryInfo(directory), new FileInfo(dependencies), new FileInfo(lockFile), new FileInfo(paketExe));
                return true;
            }

            repository = null;
            return false;
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            this.DotnetRestore.Dispose();
            this.EmptyDiff.Dispose();
            this.IsOnMaster.Dispose();
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw new System.ObjectDisposedException(this.GetType().FullName);
            }
        }
    }
}