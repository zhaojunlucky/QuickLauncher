using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QuickLauncher
{
    class OpenEditorCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private readonly Action _action;

        public OpenEditorCommand(Action action)
        {
            _action = action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if(_action != null)
            {
                _action();
            }
        }
    }
}
