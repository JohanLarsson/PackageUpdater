namespace PackageUpdater
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Gu.Reactive;
    using Gu.Wpf.Reactive;

    public sealed class TaskListViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly MappingView<Repository, TaskViewModel> mapped;
        private TaskViewModel selectedTask;
        private bool disposed;

        public TaskListViewModel(ReadOnlyObservableCollection<Repository> repositories)
        {
            this.mapped = repositories.AsMappingView(
                x => new TaskViewModel(x, this),
                x => x.Dispose());
            this.Tasks = this.mapped.AsReadOnlyFilteredView(
                x => x.Task != null,
                this.mapped.ObserveItemPropertyChangedSlim(x => x.Task));
            this.UpdateAllCommand = new AsyncCommand(() => this.UpdateAllAsync());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand UpdateAllCommand { get; }

        public IReadOnlyView<TaskViewModel> Tasks { get; }

        public ITaskInfo TaskInfo { get; } = new SyncGitInfo();

        public TaskViewModel SelectedTask
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

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            (this.UpdateAllCommand as IDisposable)?.Dispose();
            this.Tasks?.Dispose();
            this.mapped?.Dispose();
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async Task UpdateAllAsync()
        {
            await Task.WhenAll(this.Tasks.Select(x => x.Task.RunAsync()));
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