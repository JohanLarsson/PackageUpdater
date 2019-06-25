namespace PackageUpdater
{
    public class RepositoryPackageUpdate
    {
        public Repository Repository { get; }

        public BatchProcess Process { get; }

        private RepositoryPackageUpdate(Repository repository, BatchProcess process)
        {
            this.Repository = repository;
            this.Process = process;
        }
        public static bool TryCreate(Repository repository, string @group, string packageId, out RepositoryPackageUpdate result)
        {
            if (PaketUpdate.TryCreate(repository, group, packageId, out var update))
            {
                result = new RepositoryPackageUpdate(
                    repository,
                    new BatchProcess(
                        new GitAssertEmptyDiff(repository.GitDirectory),
                        new GitAssertIsOnMaster(repository.GitDirectory),
                        new GitFetchOrigin(repository.GitDirectory),
                        update,
                        new GitCleanDxf(repository.GitDirectory),
                        new DotnetRestore(repository.GitDirectory)));
                return true;
            }

            result = null;
            return false;
        }
    }
}