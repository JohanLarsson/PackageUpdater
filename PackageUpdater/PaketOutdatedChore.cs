namespace PackageUpdater
{
    using System.Collections.ObjectModel;

    public class PaketOutdatedChore : AbstractChore
    {
        public PaketOutdatedChore(ReadOnlyObservableCollection<Repository> repositories)
            : base(repositories)
        {
        }

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