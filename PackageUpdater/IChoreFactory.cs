namespace PackageUpdater
{
    using System.ComponentModel;

    public interface IChoreFactory : INotifyPropertyChanged
    {
        Batch CreateBatch(Repository repository);
    }
}