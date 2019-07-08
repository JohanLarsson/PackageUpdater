namespace PackageUpdater
{
    using System.Collections.ObjectModel;

    public class UpdatePackageChore : AbstractChore
    {
        private string group;
        private string packageId;

        public UpdatePackageChore(ReadOnlyObservableCollection<Repository> repositories)
            : base(repositories)
        {
        }

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

        public override Batch CreateBatch(Repository repository)
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
    }
}