namespace PackageUpdater
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using Gu.Reactive;
    using Gu.Wpf.Reactive;

    public sealed class Batch : AbstractTask, IDisposable
    {
        private readonly Condition canRun;
        private AbstractTask current;
        private bool disposed;

        public Batch(params AbstractTask[] steps)
        {
            this.Steps = new ReadOnlyObservableCollection<AbstractTask>(new ObservableCollection<AbstractTask>(steps));
            this.canRun = new Condition(
                () => this.Status != Status.Running,
                this.ObservePropertyChangedSlim(x => x.Status));
            this.StartCommand = new AsyncCommand(this.RunAsync, this.canRun);
        }

        public override AsyncCommand StartCommand { get; }

        public ReadOnlyObservableCollection<AbstractTask> Steps { get; }

        public override string DisplayText => string.Join(",", this.Steps.Select(x => x.DisplayText));

        public AbstractTask Current
        {
            get => this.current;
            set
            {
                if (ReferenceEquals(value, this.current))
                {
                    return;
                }

                this.current = value;
                this.OnPropertyChanged();
            }
        }

        public override async Task RunAsync()
        {
            this.ThrowIfDisposed();
            this.Status = Status.Running;
            this.Exception = null;
            foreach (var step in this.Steps)
            {
                step.Reset();
            }

            foreach (var step in this.Steps)
            {
                this.Current = step;
                try
                {
                    await step.RunAsync().ConfigureAwait(false);
                    if (step.Status == Status.Error)
                    {
                        this.Status = Status.Error;
                        return;
                    }
                }
                catch (Exception e)
                {
                    this.Status = Status.Error;
                    this.Exception = e;
                    return;
                }
            }

            this.Status = Status.Success;
        }

        public override void Reset()
        {
            this.ThrowIfDisposed();
            base.Reset();
            foreach (var step in this.Steps)
            {
                step.Reset();
            }
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            this.canRun.Dispose();
            this.StartCommand.Dispose();
            foreach (var step in Steps)
            {
                (step as IDisposable)?.Dispose();
            }
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