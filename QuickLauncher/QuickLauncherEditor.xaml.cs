using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using QuickLauncher.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace QuickLauncher
{
    /// <summary>
    /// Interaction logic for QuickLancherEditor.xaml
    /// </summary>
    public partial class QuickLancherEditor : MetroWindow
    {
        public delegate void AddedNewQuickCommandHandler(Model.QuickCommand command);
        public event AddedNewQuickCommandHandler AddedNewQuickCommand;
        private QuickCommand qCommand = null;

        public QuickLancherEditor()
        {
            InitializeComponent();
            QCommand = new QuickCommand();
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            var dbContext = QuickCommandContext.Instance;
            QCommand.Alias = alias.Text.Trim();
            QCommand.Path = path.Text.Trim();
            QCommand.Command = command.Text.Trim();
            QCommand.WorkDirectory = workDir.Text.Trim();
            if (QCommand.Alias.Length == 0)
            {
                DialogUtil.showWarning(this, "Alias can't be empty.");
                return;
            }
            else
            {
                var existingComm = from b in dbContext.QuickCommands
                                   where b.Alias == QCommand.Alias
                                   select b;
                bool error = false;
                foreach(var comm in existingComm)
                {
                    if(comm.UUID != qCommand.UUID)
                    {
                        error = true;
                        break;
                    }
                }

                if (error)
                {
                    DialogUtil.showWarning(this, "Alias already exists.");
                    return;
                }
            }

            if(QCommand.Path.Length == 0)
            {
                DialogUtil.showWarning(this, "Path can't be empty.");
                return;
            }
            else
            {
                if(!File.Exists(QCommand.Path) && !Directory.Exists(QCommand.Path))
                {
                    DialogUtil.showWarning(this, "Path doesn't exist.");
                    return;
                }
            }
            if(QCommand.WorkDirectory.Trim().Length == 0)
            {
                qCommand.WorkDirectory = FileUtil.getParentDir(path.Text);
            }
            if (QCommand.WorkDirectory.Trim().Length == 0)
            {
                DialogUtil.showWarning(this, "Working directory can't be empty.");
                return;
            }
            try
            {
                QuickCommand qc = dbContext.QuickCommands.SingleOrDefault(b => b.UUID == QCommand.UUID);
                if (qc != null)
                {
                    qc.Alias = QCommand.Alias;
                    qc.Path = QCommand.Path;
                    qc.Command = QCommand.Command;
                    qc.WorkDirectory = QCommand.WorkDirectory;
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
                
               
                this.Close();
            }
            catch(Exception ee)
            {
                DialogUtil.showError(this, ee.Message);
            }
           
        }

        public QuickCommand QCommand
        {
            set
            {
                qCommand = value;
                alias.Text = qCommand.Alias;
                path.Text = qCommand.Path;
                command.Text = qCommand.Command;
                workDir.Text = qCommand.WorkDirectory;
            }
            get
            {
                return qCommand;
            }
        }

        private string getCommandDirectory()
        {
            if (QCommand != null && QCommand.Path != null && QCommand.Path != "")
            {
                var fileInfo = new FileInfo(qCommand.Path);

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
        private void browse_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog fd = new System.Windows.Forms.OpenFileDialog();
            fd.Multiselect = false;
            fd.InitialDirectory = getCommandDirectory();
            
            
            if(fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                path.Text = fd.FileName;
                
                if (alias.Text.Trim().Length == 0)
                {
                    alias.Text = FileUtil.getFileNameNoExt(fd.SafeFileName);
                    
                }

                workDir.Text = FileUtil.getParentDir(path.Text);
            }
            this.Focus();
        }

        private void workDirBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.ShowNewFolderButton = true;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                workDir.Text = dlg.SelectedPath;
            }
            this.Focus();
        }
    }
}
