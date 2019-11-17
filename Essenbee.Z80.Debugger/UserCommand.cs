using System;
using System.Windows.Input;

namespace Essenbee.Z80.Debugger
{
    public class UserCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private readonly Func<bool> _canExecute;
        private readonly Action _execute;

        public UserCommand(Func<bool> canExecute, Action execute)
        {
            _canExecute = canExecute;
            _execute = execute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute();
        }

        public void Execute(object parameter)
        {
            _execute();
        }

        public void RefreshCanExecute()
        {
            CanExecuteChanged(this, new EventArgs());
        }

    }
}
