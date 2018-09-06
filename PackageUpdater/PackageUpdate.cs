namespace PackageUpdater
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    public class PackageUpdate : INotifyPropertyChanged
    {
        private Process updateProcess;

        public PackageUpdate(Repository repository, string package)
        {
            this.Repository = repository;
            this.Package = package;
            this.UpdateCommand = new RelayCommand(
                _ => this.Run(),
                _ => this.UpdateProcess == null && !string.IsNullOrWhiteSpace(package));
            this.DeleteDotVsFolderCommand = new RelayCommand(
                _ => this.Repository.DotVsDirectory?.Delete(true),
                _ => this.Repository.DotVsDirectory != null);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Repository Repository { get; }

        public string Package { get; }

        public ICommand UpdateCommand { get; }

        public ICommand DeleteDotVsFolderCommand { get; }

        public Process UpdateProcess
        {
            get => this.updateProcess;
            private set
            {
                if (ReferenceEquals(value, this.updateProcess))
                {
                    return;
                }

                this.updateProcess = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Run()
        {
            this.UpdateProcess = Process.Start(this.Repository.PaketExe.FullName, $"update {this.Package}");
            this.updateProcess.OnExit(() => this.UpdateProcess = null);
        }
    }
}