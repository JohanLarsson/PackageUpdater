namespace PackageUpdater
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Threading;
    using Gu.Reactive;
    using Gu.Wpf.Reactive;

    public abstract class AbstractCliTask : AbstractTask, IDisposable
    {
        private readonly SerialDisposable<Process?> serialDisposable = new SerialDisposable<Process?>();
        private readonly ObservableCollection<DataReceivedEventArgs> data = new ObservableCollection<DataReceivedEventArgs>();
        private readonly ObservableCollection<DataReceivedEventArgs> errors = new ObservableCollection<DataReceivedEventArgs>();
        private bool disposed;

        protected AbstractCliTask(string exe, string arguments, DirectoryInfo workingDirectory)
        {
            this.Data = new ReadOnlyObservableCollection<DataReceivedEventArgs>(this.data);
            this.Errors = new ReadOnlyObservableCollection<DataReceivedEventArgs>(this.errors);
            this.Exe = exe;
            this.Arguments = arguments;
            this.DisplayText = Regex.Match(exe, "(?<name>\\w+)\\.exe").Groups["name"].Value + (string.IsNullOrWhiteSpace(arguments) ? string.Empty : $" {arguments}");
            this.WorkingDirectory = workingDirectory;
            this.StartCommand = new AsyncCommand(() => this.RunAsync());
        }

        public string Exe { get; }

        public string Arguments { get; }

        public DirectoryInfo WorkingDirectory { get; }

        public override AsyncCommand StartCommand { get; }

        public ReadOnlyObservableCollection<DataReceivedEventArgs> Data { get; }

        public ReadOnlyObservableCollection<DataReceivedEventArgs> Errors { get; }

        public override string DisplayText { get; }

        public override Task RunAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            this.Reset();
            this.Status = Status.Running;
            this.Exception = null;
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
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                this.Exception = e;
                this.Status = Status.Error;
                this.serialDisposable.Disposable = null;
                tcs.SetException(e);
            }

            return tcs.Task;

            void OnDataReceived(object? sender, DataReceivedEventArgs e)
            {
                if (e.Data != null)
                {
                    _ = Application.Current.Dispatcher.BeginInvoke(new Action(() => this.data.Add(e)), DispatcherPriority.Normal);
                }
            }

            void OnErrorReceived(object? sender, DataReceivedEventArgs e)
            {
                if (e.Data != null)
                {
                    _ = Application.Current.Dispatcher.BeginInvoke(new Action(() => this.errors.Add(e)), DispatcherPriority.Normal);
                }
            }

            void OnProcessOnExited(object? sender, EventArgs e)
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

        public override void Reset()
        {
            base.Reset();
            this.data.Clear();
            this.errors.Clear();
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
    }
}