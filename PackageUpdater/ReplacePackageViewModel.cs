namespace PackageUpdater
{
    using System;
    using System.ComponentModel;
    using System.Reactive.Linq;
    using System.Runtime.CompilerServices;
    using Gu.Reactive;

    public sealed class ReplacePackageViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly SerialDisposable<BatchProcess> process = new SerialDisposable<BatchProcess>();
        private readonly IDisposable disposable;
        private bool disposed;
        private AbstractProcess selectedStep;

        public ReplacePackageViewModel(Repository repository, ReplacePackagesViewModel replacePackages)
        {
            this.Repository = repository;
            this.disposable = Observable.Merge(
                                            replacePackages.ObservePropertyChangedSlim(x => x.NewPackageId),
                                            replacePackages.ObservePropertyChangedSlim(x => x.OldPackageId))
                                        .Subscribe(_ =>
                                        {
                                            if (PaketReplace.TryCreate(repository, replacePackages.NewPackageId, replacePackages.OldPackageId, out var replace) &&
                                                PaketInstall.TryCreate(repository, out var paketInstall))
                                            {
                                                this.Process = new BatchProcess(
                                                    new GitAssertEmptyDiff(repository.Directory),
                                                    new GitAssertIsOnMaster(repository.Directory),
                                                    new GitPullFastForwardOnly(repository.Directory),
                                                    new GitCleanDxf(repository.Directory),
                                                    replace,
                                                    paketInstall,
                                                    new DotnetRestore(repository.Directory));
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