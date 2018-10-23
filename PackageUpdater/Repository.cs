namespace PackageUpdater
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
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
            this.CleanCommand = new RelayCommand(
                _ => this.Clean(),
                _ => this.DotVsDirectory != null);
            this.RestoreCommand = new RelayCommand(_ => this.Restore(), _ => true);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public FileInfo Sln { get; }

        public FileInfo Dependencies { get; }

        public FileInfo LockFile { get; }

        public FileInfo PaketExe { get; }

        public DirectoryInfo RootDirectory => this.Sln.Directory;

        public ICommand CleanCommand { get; }

        public ICommand RestoreCommand { get; }

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

        private void Clean()
        {
            try
            {
                this.DotVsDirectory?.Delete(true);
                if (this.Sln.Directory is DirectoryInfo dir)
                {
                    foreach (var csproj in dir.EnumerateFiles("*.csproj", SearchOption.AllDirectories))
                    {
                        if (csproj.DirectoryName is string directoryName)
                        {
                            Directory.Delete(Path.Combine(directoryName, "bin"));
                            Directory.Delete(Path.Combine(directoryName, "obj"));
                        }
                    }
                }


                this.OnPropertyChanged(nameof(this.DotVsDirectory));
            }
            catch
            {
                // just swallowing
            }
        }

        private void Restore()
        {
            var process = new Process
            {

                StartInfo = new ProcessStartInfo("CMD.exe")
                {
                    Arguments = "dotnet restore",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = this.Sln.DirectoryName,
                },
            };

            process.Start();
        }
    }
}