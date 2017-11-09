using MahApps.Metro.Controls;
using QuickLauncher.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for EnvironmentVariableEditor.xaml
    /// </summary>
    public partial class EnvironmentVariableEditor : MetroWindow
    {

        private ObservableCollection<QuickCommandEnvConfig> quickCommands = new ObservableCollection<QuickCommandEnvConfig>();
        private List<QuickCommandEnvConfig> deleted = new List<QuickCommandEnvConfig>();
        private QuickCommandContext dbContext = QuickCommandContext.Instance;
        public EnvironmentVariableEditor(QuickCommand qc)
        {
            InitializeComponent();
            QuickCommandObject = qc;
            if (QuickCommandObject == null)
            {
                MessageBox.Show("Null QuickCommand object.", "Error");
                return;
            }
            if(QuickCommandObject.QuickCommandEnvConfigs != null)
            {
                foreach(var o in QuickCommandObject.QuickCommandEnvConfigs)
                {
                    quickCommands.Add(o);
                }
            }
            
            this.listView.ItemsSource = quickCommands;
        }

        public QuickCommand QuickCommandObject
        {
            get; set;
        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            if(checkPassed())
            {
                foreach(var o in deleted)
                {
                    dbContext.QuickCommandEnvConfigs.Remove(o);
                }
                foreach(var o in quickCommands)
                {
                    o.EnvKey = o.EnvKey.Trim();
                    if(o.Id <= 0)
                    {
                        dbContext.QuickCommandEnvConfigs.Add(o);
                    }
                }
                dbContext.SaveChanges();

                QuickCommandObject.QuickCommandEnvConfigs = quickCommands.ToList();
                MessageBox.Show("Data saved successfully.","Notice");
                this.Close();
            }
        }

        private bool checkPassed()
        {
            var pass = true;
            string message = "";
            for(var i = 0; i < quickCommands.Count; ++i)
            {
                var o = quickCommands[i];
                if(o.EnvKey.Trim().Length == 0)
                {
                    message += "Environment key is null or empty.";
                }
                if (o.EnvValue.Trim().Length == 0)
                {
                    message += "Environment value is null or empty.";
                }
                if(message.Length > 0)
                {
                    message = "Item " + (i + 1) + " : " + message;
                    MessageBox.Show(message, "Error",MessageBoxButton.OK,MessageBoxImage.Error);
                    pass = false;
                    break;
                }
            }
            return pass;
        }

        private void browse_Click(object sender, RoutedEventArgs e)
        {
            QuickCommandEnvConfig qc = ((System.Windows.Controls.Button)sender).Tag as QuickCommandEnvConfig;

            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.ShowNewFolderButton = true;

            if(Directory.Exists(qc.EnvValue))
            {
                dlg.SelectedPath = qc.EnvValue;
            }
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                qc.EnvValue = dlg.SelectedPath ;
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            QuickCommandEnvConfig qc = ((System.Windows.Controls.Button)sender).Tag as QuickCommandEnvConfig;
            if (qc != null)
            {
                deleted.Add(qc);
                quickCommands.Remove(qc);
            }
        }

        private void add_Click(object sender, RoutedEventArgs e)
        {
            var qcEnv = new QuickCommandEnvConfig();
            qcEnv.Id = -1;
            qcEnv.ParentId = QuickCommandObject.UUID;
            quickCommands.Add(qcEnv);
        }

        private void envValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            var o = sender as TextBox;
            var text = o.Text.Trim();
            if (!text.EndsWith("\\") && Directory.Exists(text))
            {
                o.Text = text + "\\";
            }
        }
    }
}
