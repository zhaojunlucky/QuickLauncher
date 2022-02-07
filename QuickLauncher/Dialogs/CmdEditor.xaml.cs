using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using QuickLauncher.Command;
using QuickLauncher.Model;

namespace QuickLauncher.Dialogs
{
    /// <summary>
    /// Interaction logic for CmdEditor.xaml
    /// </summary>
    public partial class CmdEditor : CustomDialog
    {
        public CmdEditor(MetroWindow parent, MetroDialogSettings mySettings, QuickCommand command) :
            base(parent, mySettings)
        {
            InitializeComponent();
            this.DataContext = new CmdEditorModel(command)
            {
                CloseCommand = new SimpleCommand(x => true, x => parent.HideMetroDialogAsync(this))
            };
        }
    }
}
