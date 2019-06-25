namespace PackageUpdater
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows;

    public abstract class AbstractProcess : INotifyPropertyChanged
    {
        private Status status = Status.Waiting;

        protected AbstractProcess(string exe, string arguments, DirectoryInfo workingDirectory)
        {
            this.Exe = exe;
            this.Arguments = arguments;
            this.WorkingDirectory = workingDirectory;
            this.StartCommand = new AsyncCommand(() => this.RunAsync());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Exe { get; }

        public string Arguments { get; }

        public DirectoryInfo WorkingDirectory { get; }

        public AsyncCommand StartCommand { get; }

        public ObservableCollection<DataReceivedEventArgs> Datas { get; } = new ObservableCollection<DataReceivedEventArgs>();

        public ObservableCollection<DataReceivedEventArgs> Errors { get; } = new ObservableCollection<DataReceivedEventArgs>();

        public Status Status
        {
            get => this.status;
            protected set
            {
                if (value == this.status)
                {
                    return;
                }

                this.status = value;
                this.OnPropertyChanged();
            }
        }

        public virtual Task RunAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            this.Status = Status.Running;
            this.Datas.Clear();
            this.Errors.Clear();
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = this.Exe,
                    Arguments = this.Arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = this.WorkingDirectory.FullName,
                },
                EnableRaisingEvents = true,
            };

            process.OutputDataReceived += OnDataReceived;
            process.ErrorDataReceived += OnErrorReceived;
            process.Exited += OnProcessOnExited;
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            return tcs.Task;

            void OnDataReceived(object sender, DataReceivedEventArgs e)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => this.Datas.Add(e)));
            }

            void OnErrorReceived(object sender, DataReceivedEventArgs e)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => this.Errors.Add(e)));
            }

            void OnProcessOnExited(object sender, EventArgs e)
            {
                process.OutputDataReceived -= OnDataReceived;
                process.ErrorDataReceived -= OnErrorReceived;
                process.Exited -= OnProcessOnExited;
                process.Dispose();
                // Huge hack below to make sure all events are in collections before exiting.
                Application.Current.Dispatcher.Invoke(() => { });
                this.Status = this.Errors.Any() ? Status.Error : Status.Success;
                tcs.SetResult(true);
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}