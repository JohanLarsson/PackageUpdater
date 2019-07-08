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
        private bool disposed;

        public ViewModel()
        {
            this.AllRepositories = new ReadOnlyObservableCollection<Repository>(this.allRepositories);
            this.Chores = new Chores(this.AllRepositories);
            this.BrowseForGitDirectoryCommand = new RelayCommand(() => this.BrowseForGitDirectory());
            this.UpdateRepositories();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand BrowseForGitDirectoryCommand { get; }

        public ReadOnlyObservableCollection<Repository> AllRepositories { get; }

        public Chores Chores { get; }

        public DebugViewModel Debug { get; } = new DebugViewModel();

        public string GitDirectory
        {
            get => Properties.Settings.Default.GitDirectory;
            set
            {
                if (value == Properties.Settings.Default.GitDirectory)
                {
                    return;
                }

                Properties.Settings.Default.GitDirectory = value;
                this.OnPropertyChanged();
                this.UpdateRepositories();
            }
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            this.Chores.Dispose();
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

            var excluded = Properties.Settings.Default.ExcludedDirectories?.Cast<string>() ??
                           Enumerable.Empty<string>();
            this.allRepositories.Clear();
            if (this.GitDirectory is string path &&
                Directory.Exists(path))
            {
                foreach (var directory in Directory.EnumerateDirectories(path))
                {
                    if (excluded.Any(x => x == Path.GetDirectoryName(directory)))
                    {
                        continue;
                    }

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

        private void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw new System.ObjectDisposedException(this.GetType().FullName);
            }
        }
    }
}
