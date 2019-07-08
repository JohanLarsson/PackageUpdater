namespace PackageUpdater
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Gu.Wpf.Reactive;

    public sealed class Chores : INotifyPropertyChanged, IDisposable
    {
        private bool disposed;
        private AbstractChore selectedChore;

        public Chores(ReadOnlyObservableCollection<Repository> repositories)
        {
            this.Items = new ReadOnlyObservableCollection<AbstractChore>(
                new ObservableCollection<AbstractChore>
                {
                    new SyncGitChore(repositories),
                    new UpdatePackageChore(repositories),
                    new PaketOutdatedChore(repositories),
                });
            this.selectedChore = this.Items.First();
            this.RunAllCommand = new AsyncCommand(() => this.RunAllAsync());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand RunAllCommand { get; }

        public ReadOnlyObservableCollection<AbstractChore> Items { get; }

        public AbstractChore SelectedChore
        {
            get => this.selectedChore;
            set
            {
                if (ReferenceEquals(value, this.selectedChore))
                {
                    return;
                }

                this.selectedChore = value;
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
            (this.RunAllCommand as IDisposable)?.Dispose();
            foreach (var item in this.Items)
            {
                item.Dispose();
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async Task RunAllAsync()
        {
            if (this.SelectedChore is AbstractChore chore)
            {
                await Task.WhenAll(chore.Tasks.Select(x => x.Batch.RunAsync()));
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