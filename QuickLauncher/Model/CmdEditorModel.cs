using Microsoft.Win32;
using QuickLauncher.Command;
using QuickLauncher.Miscs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace QuickLauncher.Model
{
    internal class CmdEditorModel : AbstractMetroModel
    {
        private readonly PreDefinedCommand preDefinedCommand = GlobalSetting.Instance.GetPreDefinedCommand();

        private ICommand chooseApplicationCmd;
        private ICommand chooseWorkingDirCmd;
        private ICommand chooseIconCmd;
        private ICommand saveCmd;
        private ICommand autoSetAliasCmd;
        private ICommand autoSetWorkingDirCmd;

        internal CmdEditorModel(QuickCommand quickCommand)
        {
            if (quickCommand == null)
            {
                QCommand = new QuickCommand(true);
                Title = "Create Quick Command";
            }
            else
            {
                QCommand = QuickCommand.Copy(quickCommand);
                Title = "Edit Quick Command";
            }
            DefaultCmdDropDownMenuItemCommand = new SimpleCommand(o => true, x =>
            {
                DefaultQuickCommandSelected(x);
            });
        }


        public QuickCommand QCommand
        {
            get;
            set;
        }

        public string Title
        {
            get;
            set;
        }

        public override string this[string columnName] => string.Empty;

        public override string Error => string.Empty;

        public Visibility DefaultCmdDropDownVisibility
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

        public ICommand DefaultCmdDropDownMenuItemCommand { get; }

        private string GetCommandDirectory()
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

        public ICommand ChooseApplicationCmd
        {
            get
            {
                if (chooseApplicationCmd == null)
                {
                    chooseApplicationCmd = new SimpleCommand(o => true, x =>
                    {
                        OpenFileDialog fd = new OpenFileDialog
                        {
                            Multiselect = false,
                            InitialDirectory = GetCommandDirectory()
                        };

                        if (fd.ShowDialog() == true)
                        {
                            QCommand.Path = fd.FileName;
                        }
                    });
                }

                return chooseApplicationCmd;
            }
        }

        public ICommand ChooseWorkingDirCmd
        {
            get
            {
                if (chooseWorkingDirCmd == null)
                {
                    chooseWorkingDirCmd = new SimpleCommand(o => true, x =>
                    {
                        var dlg = new System.Windows.Forms.FolderBrowserDialog
                        {
                            ShowNewFolderButton = true
                        };
                        if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            QCommand.WorkDirectory = dlg.SelectedPath;
                        }
                    });
                }

                return chooseWorkingDirCmd;
            }
        }

        public ICommand ChooseIconCmd
        {
            get
            {
                if (chooseIconCmd == null)
                {
                    chooseIconCmd = new SimpleCommand(o => true, x =>
                    {
                        OpenFileDialog fd = new OpenFileDialog
                        {
                            Multiselect = false,
                            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                            Filter =
                                "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png, *.ico) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png; *.ico"
                        };

                        if (fd.ShowDialog() == true)
                        {
                            var fileInfo = new FileInfo(fd.FileName);
                            if (fileInfo.Length > 200 * 1024)
                            {
                                DialogUtil.ShowError(this, "The image size should no more than 200KB");
                            }
                            else
                            {
                                var img = new BitmapImage(new Uri(fd.FileName));
                                if (img.PixelHeight > 128 || img.PixelWidth > 128)
                                {
                                    DialogUtil.ShowError(this,
                                        "The image width and height should mo more than 128 pixel");
                                }
                                else
                                {
                                    QCommand.Img = img;
                                }
                            }
                        }
                    });
                }

                return chooseIconCmd;
            }
        }

        public ICommand SaveCmd
        {
            get
            {
                if (saveCmd == null)
                {
                    saveCmd = new SimpleCommand(o => true, x =>
                    {
                        if (QCommand.Error != null)
                        {
                            DialogUtil.ShowError(this, QCommand.Error);
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
                                    qc.IsAutoStart = QCommand.IsAutoStart;
                                    dbContext.SaveChanges();
                                }
                                else
                                {
                                    dbContext.QuickCommands.Add(QCommand);
                                    dbContext.SaveChanges();
                                }

                            }
                            catch (Exception ee)
                            {
                                Trace.TraceError(ee.Message);
                                Trace.TraceError(ee.StackTrace);
                                DialogUtil.ShowError(this, ee.Message);
                            }
                        }
                        CloseDialog();
                    });
                }

                return saveCmd;
            }
        }

        private void DefaultQuickCommandSelected(object o)
        {
            if (o is QuickCommand defaultCmd)
            {
                QCommand.Path = defaultCmd.Path ?? "";
                QCommand.Alias = defaultCmd.Alias ?? "";
                QCommand.Command = defaultCmd.Command ?? "";
                QCommand.WorkDirectory = FileUtil.getDirectoryOfFile(QCommand.Path);
            }
        }

        public ICommand AutoSetAliasCmd
        {
            get
            {
                if (autoSetAliasCmd == null)
                {
                    autoSetAliasCmd = new SimpleCommand(o => true, x =>
                    {
                        if (!string.IsNullOrEmpty(QCommand.Path))
                        {
                            QCommand.Alias = FileUtil.getFileNameNoExt(QCommand.Path);
                        }
                    });
                }

                return autoSetAliasCmd;
            }
        }

        public ICommand AutoSetWorkingDirCmd
        {
            get
            {
                if (autoSetWorkingDirCmd == null)
                {
                    autoSetWorkingDirCmd = new SimpleCommand(o => true, x =>
                    {
                        if (!string.IsNullOrEmpty(QCommand.Path))
                        {
                            QCommand.WorkDirectory = FileUtil.getDirectoryOfFile(QCommand.Path);
                        }
                    });
                }
                return autoSetWorkingDirCmd;
            }
        }
    }
}
