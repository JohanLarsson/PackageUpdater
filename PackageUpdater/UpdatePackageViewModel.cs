namespace PackageUpdater
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Input;

    public class UpdatePackageViewModel : INotifyPropertyChanged
    {
        private readonly ObservableCollection<RepositoryPackageUpdate> packageUpdates = new ObservableCollection<RepositoryPackageUpdate>();
        private readonly IEnumerable<Repository> repositories;
        private string group;
        private string packageId;
        private RepositoryPackageUpdate selectedPackage;

        public UpdatePackageViewModel(IEnumerable<Repository> repositories)
        {
            this.repositories = repositories;
            this.PackageUpdates = new ReadOnlyObservableCollection<RepositoryPackageUpdate>(this.packageUpdates);
            this.UpdateAllCommand = new AsyncCommand(
                () => this.UpdateAllAsync(),
                () => this.PackageUpdates.Any());
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
            get => this.@group;
            set
            {
                if (value == this.@group)
                {
                    return;
                }

                this.@group = value;
                this.OnPropertyChanged();
                this.Update();
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
                this.Update();
            }
        }

        public ReadOnlyObservableCollection<RepositoryPackageUpdate> PackageUpdates { get; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Update()
        {
            this.SelectedPackage = null;
            this.packageUpdates.Clear();
            foreach (var repository in this.repositories)
            {
                if (RepositoryPackageUpdate.TryCreate(repository, this.group, this.packageId, out var update))
                {
                    this.packageUpdates.Add(update);
                }
            }
        }

        private async Task UpdateAllAsync()
        {
            await Task.WhenAll(this.PackageUpdates.Select(x => x.Process.RunAsync()));
        }
    }
}