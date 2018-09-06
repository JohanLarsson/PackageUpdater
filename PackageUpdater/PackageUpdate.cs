namespace PackageUpdater
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    public class PackageUpdate : INotifyPropertyChanged
    {
        public PackageUpdate(Repository repository, string package)
        {
            this.Repository = repository;
            this.UpdateProcess = new UpdateProcess(repository, package);

            this.DeleteDotVsFolderCommand = new RelayCommand(
                _ =>
                {
                    try
                    {
                        this.Repository.DotVsDirectory?.Delete(true);
                    }
                    catch
                    {
                        // just swallowing
                    }
                },
                _ => this.Repository.DotVsDirectory != null);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Repository Repository { get; }

        public UpdateProcess UpdateProcess { get; }

        public ICommand DeleteDotVsFolderCommand { get; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}