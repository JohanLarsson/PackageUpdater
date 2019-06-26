namespace PackageUpdater
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class UpdatePackageInfo : ITaskInfo
    {
        private string group;
        private string packageId;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Group
        {
            get => this.group;
            set
            {
                if (value == this.group)
                {
                    return;
                }

                this.group = value;
                this.OnPropertyChanged();
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
            }
        }

        public Batch CreateBatch(Repository repository)
        {
            if (PaketUpdate.TryCreate(repository, this.packageId, this.group, out var update))
            {
                return new Batch(
                    new GitAssertEmptyDiff(repository.Directory),
                    new GitAssertIsOnMaster(repository.Directory),
                    new GitPullFastForwardOnly(repository.Directory),
                    new GitCleanDxf(repository.Directory),
                    update,
                    new DotnetRestore(repository.Directory));
            }
            else
            {
                return null;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}