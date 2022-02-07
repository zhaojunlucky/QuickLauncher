using System.ComponentModel;
using System.Windows.Input;
using Utility.Model;

namespace QuickLauncher.Model
{
    internal abstract class AbstractMetroModel : AbstractNotifyPropertyChanged, IDataErrorInfo
    {
        public abstract string Error { get; }
        public abstract string this[string columnName] { get; }

        public ICommand CloseCommand
        {
            get;
            set;
        }

        public void CloseDialog()
        {
            CloseCommand?.Execute(this);
        }
    }
}
