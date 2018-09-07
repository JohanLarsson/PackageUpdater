namespace PackageUpdater
{
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.CompilerServices;

    public class PackageUpdate : INotifyPropertyChanged
    {
        public PackageUpdate(Repository repository, string group, string package)
        {
            this.Repository = repository;
            this.UpdateProcess = new UpdateProcess(repository, group, package);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Repository Repository { get; }

        public UpdateProcess UpdateProcess { get; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static bool TryCreate(Repository repository, string group, string packageId, out PackageUpdate update)
        {
            if (string.IsNullOrWhiteSpace(group) && string.IsNullOrWhiteSpace(packageId))
            {
                update = new PackageUpdate(repository, null, null);
                return true;
            }

            var deps = File.ReadAllText(repository.Dependencies.FullName);

            if (!string.IsNullOrWhiteSpace(group) &&
                !deps.Contains($"group {group}"))
            {
                    update = null;
                    return false;
            }

            if (!string.IsNullOrWhiteSpace(packageId) &&
                !deps.Contains($"nuget {packageId}"))
            {
                update = null;
                return false;
            }

            update = new PackageUpdate(repository, group, packageId);
            return true;
        }
    }
}