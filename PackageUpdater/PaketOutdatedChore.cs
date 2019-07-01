namespace PackageUpdater
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class PaketOutdatedChore : IChoreFactory
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Batch CreateBatch(Repository repository)
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}