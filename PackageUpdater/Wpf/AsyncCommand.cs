namespace PackageUpdater
{
    using System;
    using System.Threading.Tasks;
    using System.Windows.Input;

    public class AsyncCommand : ICommand
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
                this.isRunning = true;
                await this.execute();
            }
            finally
            {
                this.isRunning = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }
}