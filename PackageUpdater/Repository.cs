namespace PackageUpdater
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    public class Repository : INotifyPropertyChanged
    {
        private Repository(DirectoryInfo gitDirectory, FileInfo dependencies, FileInfo lockFile, FileInfo paketExe)
        {
            this.GitDirectory = gitDirectory;
            this.Dependencies = dependencies;
            this.LockFile = lockFile;
            this.PaketExe = paketExe;
            this.SolutionFiles = new ReadOnlyObservableCollection<FileInfo>(new ObservableCollection<FileInfo>(gitDirectory.EnumerateFiles("*.sln", SearchOption.TopDirectoryOnly)));
            this.CleanCommand = new RelayCommand(_ => this.Clean());
            this.DotnetRestore = new DotnetRestore(this.GitDirectory);
            this.EmptyDiff = new GitAssertEmptyDiff(this.GitDirectory);
            this.IsOnMaster = new GitAssertIsOnMaster(this.GitDirectory);
            Initialize();
            async void Initialize()
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

        public ICommand CleanCommand { get; }

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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Clean()
        {
            try
            {
                foreach (var sln in this.SolutionFiles)
                {
                    if (sln.Directory.EnumerateDirectories(".vs").FirstOrDefault() is DirectoryInfo dir)
                    {
                        dir.Delete();
                    }
                }

                foreach (var csproj in this.GitDirectory.EnumerateFiles("*.csproj", SearchOption.AllDirectories))
                {
                    if (csproj.DirectoryName is string directoryName)
                    {
                        Directory.Delete(Path.Combine(directoryName, "bin"));
                        Directory.Delete(Path.Combine(directoryName, "obj"));
                    }
                }
            }
            catch
            {
                // just swallowing
            }
        }
    }
}