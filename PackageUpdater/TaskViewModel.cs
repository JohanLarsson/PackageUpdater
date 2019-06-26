namespace PackageUpdater
{
    using System;
    using System.ComponentModel;
    using System.Reactive.Linq;
    using System.Runtime.CompilerServices;
    using Gu.Reactive;

    public sealed class TaskViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly SerialDisposable<Batch> process = new SerialDisposable<Batch>();
        private readonly IDisposable disposable;
        private bool disposed;
        private AbstractTask selectedStep;
        private static readonly PropertyChangedEventArgs PropertyChangedEventArgs = new PropertyChangedEventArgs(string.Empty);

        public TaskViewModel(Repository repository, TaskListViewModel taskList)
        {
            this.Repository = repository;
            this.disposable = taskList.UpdatePackageInfo.ObservePropertyChangedSlim()
                                      .StartWith(PropertyChangedEventArgs)
                                      .Subscribe(_ => this.Task = taskList.UpdatePackageInfo.CreateBatch(repository));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Repository Repository { get; }

        public Batch Task
        {
            get => this.process.Disposable;
            set
            {
                if (ReferenceEquals(value, this.process.Disposable))
                {
                    return;
                }

                this.process.Disposable = value;
                this.OnPropertyChanged();
            }
        }

        public AbstractTask SelectedStep
        {
            get => this.selectedStep;
            set
            {
                if (ReferenceEquals(value, this.selectedStep))
                {
                    return;
                }

                this.selectedStep = value;
                this.OnPropertyChanged();
            }
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            this.process.Dispose();
            this.disposable?.Dispose();
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }
    }
}