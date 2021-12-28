using System;
using System.Windows.Input;

namespace QuickLauncher
{
    class OpenEditorCommand : ICommand
    {
        private readonly Action _action;

        public OpenEditorCommand(Action action)
        {
            _action = action;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (_action != null)
            {
                _action();
            }
        }
    }
}
