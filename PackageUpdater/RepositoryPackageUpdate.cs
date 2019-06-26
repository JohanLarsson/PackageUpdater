namespace PackageUpdater
{
    using System;
    using System.ComponentModel;
    using System.Reactive.Linq;
    using System.Runtime.CompilerServices;
    using Gu.Reactive;

    public sealed class RepositoryPackageUpdate : INotifyPropertyChanged, IDisposable
    {
        private readonly SerialDisposable<BatchProcess> process = new SerialDisposable<BatchProcess>();
        private readonly IDisposable disposable;
        private bool disposed;
        private AbstractProcess selectedStep;

        public RepositoryPackageUpdate(Repository repository, UpdatePackageViewModel updatePackageViewModel)
        {
            this.Repository = repository;
            this.disposable = Observable.Merge(
                    updatePackageViewModel.ObservePropertyChangedSlim(x => x.PackageId),
                    updatePackageViewModel.ObservePropertyChangedSlim(x => x.Group))
                .Subscribe(_ =>
                {
                    if (PaketUpdate.TryCreate(repository, updatePackageViewModel.PackageId, updatePackageViewModel.Group, out var update))
                    {
                        this.Process = new BatchProcess(
                            new GitAssertEmptyDiff(repository.GitDirectory),
                            new GitAssertIsOnMaster(repository.GitDirectory),
                            new GitFetchOrigin(repository.GitDirectory),
                            update,
                            new GitCleanDxf(repository.GitDirectory),
                            new DotnetRestore(repository.GitDirectory));
                    }
                    else
                    {
                        this.Process = null;
                    }
                });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Repository Repository { get; }

        public BatchProcess Process
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

        public AbstractProcess SelectedStep
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