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

    public sealed class UpdatePackagesViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly MappingView<Repository, UpdatePackageViewModel> mapped;
        private string group;
        private string packageId;
        private UpdatePackageViewModel selectedUpdate;
        private bool disposed;

        public UpdatePackagesViewModel(ReadOnlyObservableCollection<Repository> repositories)
        {
            mapped = repositories.AsMappingView(
                x => new UpdatePackageViewModel(x, this),
                x => x.Dispose());
            this.Updates = mapped.AsReadOnlyFilteredView(
                x => x.Process != null,
                mapped.ObserveItemPropertyChangedSlim(x => x.Process));
            this.UpdateAllCommand = new AsyncCommand(() => this.UpdateAllAsync());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand UpdateAllCommand { get; }

        public IReadOnlyView<UpdatePackageViewModel> Updates { get; }

        public UpdatePackageViewModel SelectedUpdate
        {
            get => this.selectedUpdate;
            set
            {
                if (ReferenceEquals(value, this.selectedUpdate))
                {
                    return;
                }

                this.selectedUpdate = value;
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

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            (this.UpdateAllCommand as System.IDisposable)?.Dispose();
            this.Updates?.Dispose();
            this.mapped?.Dispose();
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async Task UpdateAllAsync()
        {
            await Task.WhenAll(this.Updates.Select(x => x.Process.RunAsync()));
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