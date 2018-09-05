namespace PackageUpdater
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Windows.Input;

    public class ViewModel : INotifyPropertyChanged
    {
        private string gitDirectory;
        private string packageId;

        public ViewModel()
        {
            this.BrowseForGitDirectoryCommand = new RelayCommand(_ => this.BrowseForGitDirectory());
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
        
        public string PackageId
        {
            get => this.packageId;
            set
            {
                if (value == this.packageId)
                {
                    return;
                }

                this.packageId = value;
                this.OnPropertyChanged();
                this.UpdateFiltered();
            }
        }

        public ObservableCollection<Repository> AllRepositories { get; } = new ObservableCollection<Repository>();

        public ObservableCollection<Repository> FilteredRepositories { get; } = new ObservableCollection<Repository>();

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
            this.AllRepositories.Clear();
            if (this.gitDirectory is string path &&
                Directory.Exists(path))
            {
                foreach (var directory in Directory.EnumerateDirectories(path))
                {
                    if (Repository.TryCreate(directory, out var repository))
                    {
                        this.AllRepositories.Add(repository);
                    }
                }
            }

            this.UpdateFiltered();
        }

        private void UpdateFiltered()
        {
            this.FilteredRepositories.Clear();
            foreach (var repository in this.AllRepositories)
            {
                this.FilteredRepositories.Add(repository);
            }
        }
    }
}
