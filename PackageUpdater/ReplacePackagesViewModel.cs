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

    public sealed class ReplacePackagesViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly MappingView<Repository, ReplacePackageViewModel> mapped;
        private string oldPackageId;
        private string newPackageId;
        private UpdatePackageViewModel selectedPackage;
        private bool disposed;

        public ReplacePackagesViewModel(ReadOnlyObservableCollection<Repository> repositories)
        {
            this.mapped = repositories.AsMappingView(
                x => new ReplacePackageViewModel(x, this),
                x => x.Dispose());
            this.Updates = this.mapped.AsReadOnlyFilteredView(
                x => x.Process != null,
                this.mapped.ObserveItemPropertyChangedSlim(x => x.Process));
            this.UpdateAllCommand = new AsyncCommand(() => Task.WhenAll(this.Updates.Select(x => x.Process.RunAsync())));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand UpdateAllCommand { get; }

        public IReadOnlyView<ReplacePackageViewModel> Updates { get; }

        public UpdatePackageViewModel SelectedPackage
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

        public string OldPackageId
        {
            get => this.oldPackageId;
            set
            {
                if (value == this.oldPackageId)
                {
                    return;
                }

                this.oldPackageId = value;
                this.OnPropertyChanged();
            }
        }

        public string NewPackageId
        {
            get => this.newPackageId;
            set
            {
                if (value == this.newPackageId)
                {
                    return;
                }

                this.newPackageId = value;
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

        private void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw new System.ObjectDisposedException(this.GetType().FullName);
            }
        }
    }
}