namespace PackageUpdater
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    public class UpdateProcess : INotifyPropertyChanged
    {
        private readonly Repository repository;
        private string allOutput;
        private string output;
        private string error;
        private UpdateStatus status = UpdateStatus.Waiting;

        public UpdateProcess(Repository repository, string package)
        {
            this.repository = repository;
            this.UpdateCommand = new RelayCommand(
                _ => this.Run(),
                _ => this.allOutput == null && !string.IsNullOrWhiteSpace(package));
            this.Package = package;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand UpdateCommand { get; }

        public string Package { get; }

        public string AllOutput
        {
            get => this.allOutput;
            set
            {
                if (value == this.allOutput)
                {
                    return;
                }

                this.allOutput = value;
                this.OnPropertyChanged();
            }
        }

        public string Output
        {
            get => this.output;
            private set
            {
                if (value == this.output)
                {
                    return;
                }

                this.output = value;
                this.OnPropertyChanged();
            }
        }

        public string Error
        {
            get => this.error;
            private set
            {
                if (value == this.error)
                {
                    return;
                }

                this.error = value;
                this.OnPropertyChanged();
            }
        }

        public UpdateStatus Status
        {
            get => this.status;
            private set
            {
                if (value == this.status)
                {
                    return;
                }

                this.status = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
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
            this.Status = UpdateStatus.Waiting;
            this.Output = null;
            this.Error = null;
            this.AllOutput = "Running";
            process.EnableRaisingEvents = true;
            process.OutputDataReceived += OnDataReceived;
            process.ErrorDataReceived += OnErrorReceived;
            process.Exited += OnProcessOnExited;
            process.Start();

            void OnDataReceived(object sender, DataReceivedEventArgs e)
            {
                this.Output += e.Data;
                this.AllOutput += e.Data;
            }

            void OnErrorReceived(object sender, DataReceivedEventArgs e)
            {
                this.Error += e.Data;
                this.AllOutput += e.Data;
            }

            void OnProcessOnExited(object sender, EventArgs args)
            {
                process.Exited -= OnProcessOnExited;
                this.Output = process.StandardOutput.ReadToEnd();
                this.Error = process.StandardError.ReadToEnd();
                if (!string.IsNullOrWhiteSpace(this.error))
                {
                    this.Status = UpdateStatus.Error;
                }
                else if (this.output is string text && text.Contains("Downloaded"))
                {
                    this.Status = UpdateStatus.Success;
                }
                else
                {
                    this.Status = UpdateStatus.NoChange;
                }

                this.AllOutput = this.output + Environment.NewLine + this.error;
                process.OutputDataReceived -= OnDataReceived;
                process.ErrorDataReceived -= OnErrorReceived;
                process.Dispose();
            }
        }
    }
}