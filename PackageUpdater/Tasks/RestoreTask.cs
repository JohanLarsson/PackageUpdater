namespace PackageUpdater.Tasks
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Windows;

    public class RestoreTask : INotifyPropertyChanged
    {
        private bool exited;

        public RestoreTask(DirectoryInfo directory)
        {
            this.Directory = directory;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public DirectoryInfo Directory { get; }

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

        public void Start()
        {
            this.Exited = false;
            this.Datas.Clear();
            this.Errors.Clear();
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet.exe",
                    Arguments = "restore",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = this.Directory.FullName,
                },
                EnableRaisingEvents = true,
            };

            process.OutputDataReceived += OnDataReceived;
            process.ErrorDataReceived += OnErrorReceived;
            process.Exited += OnProcessOnExited;
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            process.OutputDataReceived -= OnDataReceived;
            process.ErrorDataReceived -= OnErrorReceived;
            process.Exited -= OnProcessOnExited;
            process.Dispose();

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
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
