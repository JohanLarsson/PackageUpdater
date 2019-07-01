namespace PackageUpdater
{
    using System.ComponentModel;

    public class SyncGitChore : IChoreFactory
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Batch CreateBatch(Repository repository)
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