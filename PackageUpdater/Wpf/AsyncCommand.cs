namespace PackageUpdater
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Input;

    public class AsyncCommand : ICommand, INotifyPropertyChanged
    {
        private readonly Func<Task> execute;
        private readonly Func<bool> canExecute;
        private bool isRunning;

        public AsyncCommand(Func<Task> execute, Func<bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute ?? (() => true);
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsRunning
        {
            get => this.isRunning;
            set
            {
                if (value == this.isRunning)
                {
                    return;
                }

                this.isRunning = value;
                this.OnPropertyChanged();
            }
        }

        public bool CanExecute(object parameter)
        {
            if (parameter != null)
            {
                throw new InvalidOperationException("Expected parameter to be null.");
            }

            return !this.isRunning &&
                   this.canExecute();
        }

        public async void Execute(object parameter)
        {
            if (parameter != null)
            {
                throw new InvalidOperationException("Expected parameter to be null.");
            }

            try
            {
                this.IsRunning = true;
                await this.execute();
            }
            finally
            {
                this.IsRunning = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}