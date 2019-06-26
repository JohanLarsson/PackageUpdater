namespace PackageUpdater
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Threading;
    using Gu.Reactive;
    using Gu.Wpf.Reactive;

    public abstract class AbstractProcess : INotifyPropertyChanged, IDisposable
    {
        private readonly SerialDisposable<Process> serialDisposable = new SerialDisposable<Process>();
        private Exception exception;
        private Status status = Status.Waiting;
        private bool disposed;

        protected AbstractProcess(string exe, string arguments, DirectoryInfo workingDirectory)
        {
            this.Exe = exe;
            this.Arguments = arguments;
            this.DisplayText = Regex.Match(exe,"(?<name>\\w+)\\.exe").Groups["name"].Value + (string.IsNullOrWhiteSpace(arguments) ? string.Empty : $" {arguments}");
            this.WorkingDirectory = workingDirectory;
            this.StartCommand = new AsyncCommand(() => this.RunAsync());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Exe { get; }

        public string Arguments { get; }

        public string DisplayText { get; }

        public DirectoryInfo WorkingDirectory { get; }

        public AsyncCommand StartCommand { get; }

        public ObservableCollection<DataReceivedEventArgs> Datas { get; } = new ObservableCollection<DataReceivedEventArgs>();

        public ObservableCollection<DataReceivedEventArgs> Errors { get; } = new ObservableCollection<DataReceivedEventArgs>();

        public Exception Exception
        {
            get => this.exception;
            set
            {
                if (ReferenceEquals(value, this.exception))
                {
                    return;
                }

                this.exception = value;
                this.OnPropertyChanged();
            }
        }

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

        public void Reset()
        {
            this.Status = Status.Waiting;
        }

        public virtual Task RunAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            this.Status = Status.Running;
            this.Exception = null;
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
            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                this.serialDisposable.Disposable = process;
            }
            catch (Exception e)
            {
                this.Exception = e;
                this.Status = Status.Error;
                this.serialDisposable.Disposable = null;
            }

            return tcs.Task;

            void OnDataReceived(object sender, DataReceivedEventArgs e)
            {
                if (e.Data != null)
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => this.Datas.Add(e)), DispatcherPriority.Normal);
                }
            }

            void OnErrorReceived(object sender, DataReceivedEventArgs e)
            {
                if (e.Data != null)
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() => this.Errors.Add(e)), DispatcherPriority.Normal);
                }
            }

            void OnProcessOnExited(object sender, EventArgs e)
            {
                // Huge hack below to make sure all events are in collections before exiting.
                Application.Current.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
                process.OutputDataReceived -= OnDataReceived;
                process.ErrorDataReceived -= OnErrorReceived;
                process.Exited -= OnProcessOnExited;
                this.serialDisposable.Disposable = null;
                this.Status = this.Errors.Any() ? Status.Error : Status.Success;
                tcs.SetResult(true);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            if (disposing)
            {
                this.StartCommand.Dispose();
                this.serialDisposable.Dispose();
            }
        }

        protected virtual void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }
    }
}