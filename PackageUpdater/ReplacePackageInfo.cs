namespace PackageUpdater
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class ReplacePackageInfo : ITaskInfo
    {
        private string oldPackageId;
        private string newPackageId;

        public event PropertyChangedEventHandler PropertyChanged;

        public string OldPackageId
        {
            get => this.oldPackageId;
            set
            {
                if (value == this.oldPackageId)
                {
                    return;
                }

                this.oldPackageId = value;
                this.OnPropertyChanged();
            }
        }

        public string NewPackageId
        {
            get => this.newPackageId;
            set
            {
                if (value == this.newPackageId)
                {
                    return;
                }

                this.newPackageId = value;
                this.OnPropertyChanged();
            }
        }

        public Batch CreateBatch(Repository repository)
        {
            if (PaketReplace.TryCreate(repository, this.OldPackageId, this.NewPackageId, out var replace) &&
                PaketInstall.TryCreate(repository, out var paketInstall))
            {
                return new Batch(
                      new GitAssertEmptyDiff(repository.Directory),
                      new GitAssertIsOnMaster(repository.Directory),
                      new GitPullFastForwardOnly(repository.Directory),
                      new GitCleanDxf(repository.Directory),
                      replace,
                      paketInstall,
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