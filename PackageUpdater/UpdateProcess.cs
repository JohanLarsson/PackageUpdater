namespace PackageUpdater
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    public class UpdateProcess : INotifyPropertyChanged
    {
        private readonly Repository repository;
        private string result;

        public UpdateProcess(Repository repository, string package)
        {
            this.repository = repository;
            this.UpdateCommand = new RelayCommand(
                _ => this.Run(),
                _ => this.result == null && !string.IsNullOrWhiteSpace(package));
            this.Package = package;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand UpdateCommand { get; }

        public string Package { get; }

        public string Result
        {
            get => this.result;
            set
            {
                if (value == this.result)
                {
                    return;
                }

                this.result = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Run()
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo(this.repository.PaketExe.FullName, $" update {this.Package}")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = this.repository.RootDirectory.FullName,
                },
            };
            this.Result = "Running";
            process.EnableRaisingEvents = true;
            process.OutputDataReceived += OnDataReceived;
            process.ErrorDataReceived += OnDataReceived;
            process.Exited += OnProcessOnExited;
            process.Start();

            void OnDataReceived(object sender, DataReceivedEventArgs e)
            {
                this.Result += e.Data;
            }

            void OnProcessOnExited(object sender, EventArgs args)
            {
                this.Result = process.StandardOutput.ReadToEnd() + Environment.NewLine + process.StandardError.ReadToEnd();
                process.OutputDataReceived -= OnDataReceived;
                process.ErrorDataReceived -= OnDataReceived;
                process.Exited -= OnProcessOnExited;
            }
        }
    }
}