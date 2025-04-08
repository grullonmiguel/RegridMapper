using System.Windows.Input;

namespace RegridMapper.Core.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Func<bool> _canExecute;
        private readonly Action _execute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter = null) 
            => _canExecute == null || _canExecute.Invoke();

        public void Execute(object parameter = null) 
            => _execute.Invoke();
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Func<T, bool> _canExecute;
        private readonly Action<T> _execute;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            var strongParam = (T)parameter;

            return _canExecute == null || _canExecute(strongParam);
        }

        public void Execute(object parameter)
        {
            var strongParam = (T)parameter;

            _execute(strongParam);
        }
    }
}