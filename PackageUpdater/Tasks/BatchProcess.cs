﻿namespace PackageUpdater
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Input;

    public class BatchProcess : INotifyPropertyChanged
    {
        private AbstractProcess current;
        private Exception exception;
        private Status status = Status.Waiting;

        public BatchProcess(params AbstractProcess[] steps)
        {
            this.Steps = new ReadOnlyObservableCollection<AbstractProcess>(new ObservableCollection<AbstractProcess>(steps));
            this.StartCommand = new AsyncCommand(() => this.RunAsync());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand StartCommand { get; }

        public ReadOnlyObservableCollection<AbstractProcess> Steps { get; }

        public AbstractProcess Current
        {
            get => this.current;
            set
            {
                if (ReferenceEquals(value, this.current))
                {
                    return;
                }

                this.current = value;
                this.OnPropertyChanged();
            }
        }

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
            set
            {
                if (value == this.status)
                {
                    return;
                }

                this.status = value;
                this.OnPropertyChanged();
            }
        }

        public async Task RunAsync()
        {
            this.Status = Status.Running;
            this.Exception = null;
            foreach (var step in Steps)
            {
                this.Current = step;
                try
                {
                    await step.RunAsync();
                    if (step.Status == Status.Error)
                    {
                        this.Status = Status.Error;
                        return;
                    }
                }
                catch (Exception e)
                {
                    this.Status = Status.Error;
                    this.Exception = e;
                    return;
                }
            }

            this.Status = Status.Success;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}