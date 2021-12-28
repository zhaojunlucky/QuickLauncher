using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using QuickLauncher.Command;
using QuickLauncher.Miscs;
using QuickLauncher.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace QuickLauncher.Dialogs
{
    /// <summary>
    /// Interaction logic for CmdEditor.xaml
    /// </summary>
    public partial class CmdEditor : CustomDialog
    {
        public delegate void AddedNewQuickCommandHandler(Model.QuickCommand command);
        public event AddedNewQuickCommandHandler AddedNewQuickCommand;
        private MetroWindow parent;
        private PreDefinedCommand preDefinedCommand = GlobalSetting.Instance.GetPreDefinedCommand();

        public CmdEditor(MetroWindow parent, MetroDialogSettings mySettings, QuickCommand command) :
            base(parent, mySettings)
        {
            InitializeComponent();
            this.parent = parent;
            if (command == null)
            {
                QCommand = new QuickCommand(true);
                Title = "Create Quick Command";
            }
            else
            {
                QCommand = QuickCommand.Copy(command);
                Title = "Edit Quick Command";
            }
            DataContext = this;
            DefaultCMDDropDownMenuItemCommand = new SimpleCommand(o => true, x =>
            {
                DefaultQuickCommandSelected(x);
            });
        }

        public QuickCommand QCommand { get; set; }

        public Visibility DefaultCMDDropDownVisibility
        {
            get
            {
                return QCommand.IsNew ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public List<QuickCommand> DefaultQuickCommand
        {
            get
            {
                return preDefinedCommand.QuickCommands;
            }
        }

        public ICommand DefaultCMDDropDownMenuItemCommand { get; }

        private string getCommandDirectory()
        {
            if (QCommand != null && QCommand.Path != null && QCommand.Path != "")
            {
                var fileInfo = new FileInfo(QCommand.ExpandedPath);

                var dirInfo = fileInfo.Directory;
                while (dirInfo != null && !dirInfo.Exists)
                {
                    dirInfo = dirInfo.Parent;
                }

                if (dirInfo != null)
                {
                    return dirInfo.FullName;
                }
            }

            return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        }

        private async void Cancel_Click(object sender, RoutedEventArgs e)
        {
            await parent.HideMetroDialogAsync(this);
        }

        private void ChooseApplication_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Multiselect = false;
            fd.InitialDirectory = getCommandDirectory();

            if (fd.ShowDialog() == true)
            {
                appPath.Text = fd.FileName;
            }
        }

        private void ChooseWD_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.ShowNewFolderButton = true;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                appWorkDir.Text = dlg.SelectedPath;
            }
        }

        private void ChooseIcon_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Multiselect = false;
            fd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            fd.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png, *.ico) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png; *.ico";

            if (fd.ShowDialog() == true)
            {
                var fileInfo = new FileInfo(fd.FileName);
                if (fileInfo.Length > 200 * 1024)
                {
                    DialogUtil.showError(parent, "The image size should no more than 200KB");
                }
                else
                {
                    var img = new BitmapImage(new Uri(fd.FileName));
                    if (img.PixelHeight > 128 || img.PixelWidth > 128)
                    {
                        DialogUtil.showError(parent, "The image width and height should mo more than 128 pixel");
                    }
                    else
                    {
                        QCommand.Img = img;
                    }
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (QCommand.Error != null)
            {
                DialogUtil.showError(this.parent, QCommand.Error);
            }
            else
            {
                try
                {
                    var dbContext = QuickCommandContext.Instance;
                    QuickCommand qc = dbContext.QuickCommands.SingleOrDefault(b => b.UUID == QCommand.UUID);
                    if (qc != null)
                    {
                        qc.Alias = QCommand.Alias;
                        qc.Path = QCommand.Path;
                        qc.Command = QCommand.Command;
                        qc.WorkDirectory = QCommand.WorkDirectory;
                        qc.CustomIcon = QCommand.CustomIcon;
                        dbContext.SaveChanges();
                    }
                    else
                    {
                        dbContext.QuickCommands.Add(QCommand);
                        dbContext.SaveChanges();
                        if (AddedNewQuickCommand != null)
                        {
                            AddedNewQuickCommand(QCommand);
                        }
                    }

                    parent.HideMetroDialogAsync(this);
                }
                catch (Exception ee)
                {
                    Trace.TraceError(ee.Message);
                    Trace.TraceError(ee.StackTrace);
                    DialogUtil.showError(parent, ee.Message);
                }
            }
        }

        private void DefaultQuickCommandSelected(object o)
        {
            var defaultCmd = o as QuickCommand;
            if (defaultCmd != null)
            {
                QCommand.Path = defaultCmd.Path ?? "";
                QCommand.Alias = defaultCmd.Alias ?? "";
                QCommand.Command = defaultCmd.Command ?? "";
                QCommand.WorkDirectory = FileUtil.getDirectoryOfFile(QCommand.Path);
            }
        }

        private void appPath_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            QCommand.PathChanged();
        }

        private void AutoSetAlias_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(QCommand.Path))
            {
                QCommand.Alias = FileUtil.getFileNameNoExt(QCommand.Path);
            }
        }

        private void AutoSetWorkingDir_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(QCommand.Path))
            {
                QCommand.WorkDirectory = FileUtil.getDirectoryOfFile(QCommand.Path);
            }
        }
    }
}
