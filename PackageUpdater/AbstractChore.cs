namespace PackageUpdater
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using Gu.Reactive;

    public abstract class AbstractChore : INotifyPropertyChanged, IDisposable
    {
        private BatchViewModel selectedTask;
        private readonly MappingView<Repository, BatchViewModel> mapped;
        private bool disposed;

        protected AbstractChore(ReadOnlyObservableCollection<Repository> repositories)
        {
            this.mapped = repositories.AsMappingView(
                x => new BatchViewModel(x, this.UpdateTrigger, this.CreateBatch),
                x => x.Dispose());
            this.Tasks = this.mapped.AsReadOnlyFilteredView(
                x => x.Batch != null,
                this.mapped.ObserveItemPropertyChangedSlim(x => x.Batch));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public IReadOnlyView<BatchViewModel> Tasks { get; }

        public abstract IObservable<object> UpdateTrigger { get; }

        public BatchViewModel SelectedTask
        {
            get => this.selectedTask;
            set
            {
                if (ReferenceEquals(value, this.selectedTask))
                {
                    return;
                }

                this.selectedTask = value;
                this.OnPropertyChanged();
            }
        }

        public abstract Batch CreateBatch(Repository repository);

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            if (disposing)
            {
                this.Tasks?.Dispose();
                this.mapped?.Dispose();
            }
        }

        protected virtual void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }
    }
}