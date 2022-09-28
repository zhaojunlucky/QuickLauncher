using System;
using System.Diagnostics;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using QuickLauncher.Command;
using QuickLauncher.Model;

namespace QuickLauncher.Dialogs
{
    /// <summary>
    /// Interaction logic for CmdEditor.xaml
    /// </summary>
    public partial class CmdEditor
    {
        public delegate void OnCloseHandler(object sender);
        public event OnCloseHandler BeforeClose;

        public CmdEditor(MetroWindow parent, MetroDialogSettings mySettings, QuickCommand command) :
            base(parent, mySettings)
        {
            InitializeComponent();
            this.DataContext = new CmdEditorModel(command)
            {
                CloseCommand = new SimpleCommand(x => true, x =>
                {
                    try
                    {
                        BeforeClose?.Invoke(this);
                    }
                    catch (Exception e)
                    {
                        Trace.TraceError(e.Message);
                    }
                    
                    parent.HideMetroDialogAsync(this);
                })
            };
        }
    }
}
