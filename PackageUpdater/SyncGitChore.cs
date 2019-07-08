namespace PackageUpdater
{
    using System.Collections.ObjectModel;

    public class SyncGitChore : AbstractChore
    {
        public SyncGitChore(ReadOnlyObservableCollection<Repository> repositories)
            : base(repositories)
        {
        }

        public override Batch CreateBatch(Repository repository)
        {
            return new Batch(
                new GitFetchOrigin(repository.Directory),
                new GitAssertEmptyDiff(repository.Directory),
                new GitPullFastForwardOnly(repository.Directory),
                new GitCleanDxf(repository.Directory),
                new DotnetRestore(repository.Directory));
        }
    }
}