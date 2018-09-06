namespace PackageUpdater
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class PackageUpdate : INotifyPropertyChanged
    {
        public PackageUpdate(Repository repository, string package)
        {
            this.Repository = repository;
            this.UpdateProcess = new UpdateProcess(repository, package);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Repository Repository { get; }

        public UpdateProcess UpdateProcess { get; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}