namespace PackageUpdater
{
    using System.ComponentModel;

    public interface ITaskInfo : INotifyPropertyChanged
    {
        Batch CreateBatch(Repository repository);
    }
}