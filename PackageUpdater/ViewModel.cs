namespace PackageUpdater
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
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
            this.UpdateAllCommand = new RelayCommand(
                _ => this.UpdateAll(),
                _ => this.PackageUpdates.Any());
            this.DeleteAllDotVsFolderCommand = new RelayCommand(
                _ => this.DeleteAll(),
                _ => this.AllRepositories.Any());
        }

        private void DeleteAll()
        {
            foreach (var repository in this.AllRepositories)
            {
                repository.DeleteDotVsFolderCommand.Execute(null);
            }
        }

        private void UpdateAll()
        {
            foreach (var command in this.PackageUpdates.Select(x => x.UpdateProcess.UpdateCommand))
            {
                if (command.CanExecute(null))
                {
                    command.Execute(null);
                }
            }
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
                this.UpdateWithPackage();
            }
        }

        public ObservableCollection<Repository> AllRepositories { get; } = new ObservableCollection<Repository>();

        public ObservableCollection<PackageUpdate> PackageUpdates { get; } = new ObservableCollection<PackageUpdate>();

        public ICommand UpdateAllCommand { get; }

        public ICommand DeleteAllDotVsFolderCommand { get; }

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

            this.UpdateWithPackage();
        }

        private void UpdateWithPackage()
        {
            this.PackageUpdates.Clear();
            foreach (var repository in this.AllRepositories)
            {
                this.PackageUpdates.Add(new PackageUpdate(repository, this.packageId));
            }
        }
    }
}
