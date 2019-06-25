namespace PackageUpdater
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Gu.Reactive;
    using Gu.Wpf.Reactive;

    public sealed class UpdatePackageViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly MappingView<Repository, RepositoryPackageUpdate> mapped;

        private string group;
        private string packageId;
        private RepositoryPackageUpdate selectedPackage;
        private bool disposed;

        public UpdatePackageViewModel(ReadOnlyObservableCollection<Repository> repositories)
        {
            mapped = repositories.AsMappingView(
                x => new RepositoryPackageUpdate(x, this),
                x => x.Dispose());
            this.PackageUpdates = mapped.AsReadOnlyFilteredView(
                x => x.Process != null,
                mapped.ObserveItemPropertyChangedSlim(x => x.Process));
            this.UpdateAllCommand = new AsyncCommand(() => this.UpdateAllAsync());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand UpdateAllCommand { get; }

        public RepositoryPackageUpdate SelectedPackage
        {
            get => this.selectedPackage;
            set
            {
                if (ReferenceEquals(value, this.selectedPackage))
                {
                    return;
                }

                this.selectedPackage = value;
                this.OnPropertyChanged();
            }
        }

        public string Group
        {
            get => this.group;
            set
            {
                if (value == this.group)
                {
                    return;
                }

                this.group = value;
                this.OnPropertyChanged();
            }
        }

        public string PackageId
        {
            get => this.packageId;
            set
            {
                if (value == this.packageId)
                {
                    return;
                }

                this.packageId = value;
                this.OnPropertyChanged();
            }
        }

        public IReadOnlyView<RepositoryPackageUpdate> PackageUpdates { get; }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            (this.UpdateAllCommand as System.IDisposable)?.Dispose();
            this.PackageUpdates?.Dispose();
            this.mapped?.Dispose();
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async Task UpdateAllAsync()
        {
            await Task.WhenAll(this.PackageUpdates.Select(x => x.Process.RunAsync()));
        }

        private void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw new System.ObjectDisposedException(this.GetType().FullName);
            }
        }
    }
}