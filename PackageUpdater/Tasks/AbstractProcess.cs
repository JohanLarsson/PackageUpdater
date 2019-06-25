namespace PackageUpdater
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;

    public abstract class AbstractProcess : INotifyPropertyChanged
    {
        private bool exited;
        private bool? success;

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

        public ICommand StartCommand { get; }

        public ObservableCollection<DataReceivedEventArgs> Datas { get; } = new ObservableCollection<DataReceivedEventArgs>();

        public ObservableCollection<DataReceivedEventArgs> Errors { get; } = new ObservableCollection<DataReceivedEventArgs>();

        public bool Exited
        {
            get => this.exited;
            set
            {
                if (value == this.exited)
                {
                    return;
                }

                this.exited = value;
                this.OnPropertyChanged();
            }
        }

        public bool? Success
        {
            get => this.success;
            protected set
            {
                if (value == this.success)
                {
                    return;
                }

                this.success = value;
                this.OnPropertyChanged();
            }
        }

        public virtual Task RunAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            this.Success = null;
            this.Exited = false;
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
                this.Exited = true;
                process.OutputDataReceived -= OnDataReceived;
                process.ErrorDataReceived -= OnErrorReceived;
                process.Exited -= OnProcessOnExited;
                process.Dispose();
                // Huge hack below to make sure all events are in collections before exiting.
                Application.Current.Dispatcher.Invoke(() => { });
                tcs.SetResult(true);
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}