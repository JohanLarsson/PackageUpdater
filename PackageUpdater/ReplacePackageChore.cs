namespace PackageUpdater
{
    using System;
    using System.Collections.ObjectModel;
    using System.Reactive.Linq;
    using Gu.Reactive;

    public class ReplacePackageChore : AbstractChore
    {
        private string oldPackageId;
        private string newPackageId;

        public ReplacePackageChore(ReadOnlyObservableCollection<Repository> repositories)
            : base(repositories)
        {
            this.UpdateTrigger = Observable.Merge(
                this.ObservePropertyChangedSlim(x => x.OldPackageId, false),
                this.ObservePropertyChangedSlim(x => x.NewPackageId, false));
        }

        public override IObservable<object> UpdateTrigger { get; }

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

        public override Batch CreateBatch(Repository repository)
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
    }
}