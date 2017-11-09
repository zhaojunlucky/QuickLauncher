﻿using MahApps.Metro.Controls;
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
                MessageBox.Show("Alias can't be empty.","Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    MessageBox.Show("Alias already exists.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            if(QCommand.Path.Length == 0)
            {
                MessageBox.Show("Path can't be empty.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            else
            {
                if(!File.Exists(QCommand.Path) && !Directory.Exists(QCommand.Path))
                {
                    MessageBox.Show("Path doesn't exist.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            if(QCommand.WorkDirectory.Trim().Length == 0)
            {
                qCommand.WorkDirectory = FileUtil.getParentDir(path.Text);
            }
            if (QCommand.WorkDirectory.Trim().Length == 0)
            {
                MessageBox.Show("Working directory can't be empty.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show(ee.Message);
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
        private void browse_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog fd = new System.Windows.Forms.OpenFileDialog();
            fd.Multiselect = false;
            if(fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                path.Text = fd.FileName;
                
                if (alias.Text.Trim().Length == 0)
                {
                    alias.Text = FileUtil.getFileNameNoExt(fd.SafeFileName);
                    
                }

                workDir.Text = FileUtil.getParentDir(path.Text);
            }
        }

        private void workDirBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.ShowNewFolderButton = true;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                workDir.Text = dlg.SelectedPath;
            }
        }
    }
}
