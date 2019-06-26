namespace PackageUpdater
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using Gu.Reactive;
    using Gu.Wpf.Reactive;

    public sealed class BatchProcess : AbstractProcess, IDisposable
    {
        private readonly Condition canRun;
        private AbstractProcess current;
        private bool disposed;

        public BatchProcess(params AbstractProcess[] steps)
        {
            this.Steps = new ReadOnlyObservableCollection<AbstractProcess>(new ObservableCollection<AbstractProcess>(steps));
            this.canRun = new Condition(
                () => this.Status != Status.Running,
                this.ObservePropertyChangedSlim(x => x.Status));
            this.StartCommand = new AsyncCommand(this.RunAsync, this.canRun);
        }

        public override AsyncCommand StartCommand { get; }

        public ReadOnlyObservableCollection<AbstractProcess> Steps { get; }

        public override string DisplayText => string.Join(",", this.Steps.Select(x => x.DisplayText));

        public AbstractProcess Current
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
                    await step.RunAsync();
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