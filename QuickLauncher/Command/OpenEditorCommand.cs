using System;
using System.Windows.Input;

namespace QuickLauncher.Command
{
    class OpenEditorCommand : ICommand
    {
        private readonly Action action;

        public OpenEditorCommand(Action action)
        {
            this.action = action;
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
            if (action != null)
            {
                action();
            }
        }
    }
}
