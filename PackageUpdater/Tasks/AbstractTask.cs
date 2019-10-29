namespace PackageUpdater
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Gu.Wpf.Reactive;

    public abstract class AbstractTask : INotifyPropertyChanged
    {
        private Exception? exception;
        private Status status = Status.Waiting;

        public event PropertyChangedEventHandler? PropertyChanged;

        public abstract AsyncCommand StartCommand { get; }

        public abstract string DisplayText { get; }

        public Exception? Exception
        {
            get => this.exception;
            protected set
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

        public virtual void Reset()
        {
            this.Status = Status.Waiting;
        }

        public abstract Task RunAsync();

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}