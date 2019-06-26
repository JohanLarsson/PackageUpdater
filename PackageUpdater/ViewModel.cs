namespace PackageUpdater
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Input;
    using Gu.Wpf.Reactive;

    public sealed class ViewModel : INotifyPropertyChanged, System.IDisposable
    {
        private readonly ObservableCollection<Repository> allRepositories = new ObservableCollection<Repository>();
        private string gitDirectory;
        private bool disposed;

        public ViewModel()
        {
            this.AllRepositories = new ReadOnlyObservableCollection<Repository>(this.allRepositories);
            this.UpdatePackage = new UpdatePackageViewModel(this.AllRepositories);
            this.BrowseForGitDirectoryCommand = new RelayCommand(() => this.BrowseForGitDirectory());
            this.CleanAllCommand = new RelayCommand(
                () => this.DeleteAll(),
                () => this.AllRepositories.Any());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand BrowseForGitDirectoryCommand { get; }

        public string GitDirectory
        {
            get => this.gitDirectory;
            set
            {
                if (value == this.gitDirectory)
                {
                    return;
                }

                this.gitDirectory = value;
                this.OnPropertyChanged();
                this.UpdateRepositories();
            }
        }

        public ReadOnlyObservableCollection<Repository> AllRepositories { get; }

        public ICommand CleanAllCommand { get; }

        public UpdatePackageViewModel UpdatePackage { get; }

        public DebugViewModel Debug { get; } = new DebugViewModel();

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            this.UpdatePackage.Dispose();
            this.Debug?.Dispose();
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void BrowseForGitDirectory()
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog(Application.Current.MainWindow) == true)
            {
                this.GitDirectory = dialog.SelectedPath;
            }
        }

        private void UpdateRepositories()
        {
            foreach (var repository in this.allRepositories)
            {
                repository.Dispose();
            }

            this.allRepositories.Clear();
            if (this.gitDirectory is string path &&
                Directory.Exists(path))
            {
                foreach (var directory in Directory.EnumerateDirectories(path))
                {
                    if (Repository.TryCreate(directory, out var repository))
                    {
                        this.allRepositories.Add(repository);
                    }
                    else
                    {
                        foreach (var nested in Directory.EnumerateDirectories(directory))
                        {
                            if (Repository.TryCreate(nested, out repository))
                            {
                                this.allRepositories.Add(repository);
                            }
                        }
                    }
                }
            }
        }

        private void DeleteAll()
        {
            foreach (var repository in this.AllRepositories)
            {
                repository.CleanCommand.Execute(null);
            }
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
