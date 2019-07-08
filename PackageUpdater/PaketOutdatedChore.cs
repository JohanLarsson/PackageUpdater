namespace PackageUpdater
{
    using System;
    using System.Collections.ObjectModel;
    using System.Reactive.Linq;

    public class PaketOutdatedChore : AbstractChore
    {
        public PaketOutdatedChore(ReadOnlyObservableCollection<Repository> repositories)
            : base(repositories)
        {
        }

        public override IObservable<object> UpdateTrigger { get; } = Observable.Never<object>();

        public override Batch CreateBatch(Repository repository)
        {
            if (PaketOutdated.TryCreate(repository,  out var outdated))
            {
                return new Batch(
                    new GitAssertEmptyDiff(repository.Directory),
                    new GitPullFastForwardOnly(repository.Directory),
                    new GitCleanDxf(repository.Directory),
                    outdated);
            }
            else
            {
                return null;
            }
        }
    }
}