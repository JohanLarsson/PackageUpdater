namespace PackageUpdater
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;
    using Gu.Reactive;
    using Gu.Wpf.Reactive;

    public sealed class BatchViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly SerialDisposable<Batch> process = new SerialDisposable<Batch>();
        private readonly IDisposable disposable;
        private bool disposed;
        private AbstractTask selectedTask;

        public BatchViewModel(Repository repository, IObservable<object> trigger, Func<Repository, Batch> createBatch)
        {
            this.Repository = repository;
            this.disposable = trigger.StartWith(Unit.Default)
                                     .Subscribe(_ => this.Batch = createBatch(repository));
            this.GitExtCommitCommand = new ManualRelayCommand(
                () => Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = "c:\\Program Files (x86)\\GitExtensions\\gitex.cmd",
                        Arguments = "commit",
                        CreateNoWindow = true,
                        UseShellExecute = true,
                        WorkingDirectory = repository.Directory.FullName
                    }),
                () => File.Exists("c:\\Program Files (x86)\\GitExtensions\\gitex.cmd"));

            this.GitCheckoutResetCommand = new ManualRelayCommand(
                () => Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = "git.exe",
                        Arguments = "checkout --force -B \"master\" \"origin/master\"",
                        UseShellExecute = true,
                        WorkingDirectory = repository.Directory.FullName
                    }));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Repository Repository { get; }

        public ICommand GitExtCommitCommand { get; }
        public ICommand GitCheckoutResetCommand { get; }

        public Batch Batch
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

        public AbstractTask SelectedTask
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