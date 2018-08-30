using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Utility.Model;

namespace QuickLauncher.Model
{
    [Table("QUICK_COMMAND")]
    public class QuickCommand : AbstractNotifyPropertyChanged
    {
        private string uuid = "";
        private string alias = "";
        private string command = "";
        private string path = "";
        private ImageSource img = null;
        private Boolean isDirectory = true;
        private string workDirectory = "";
        public QuickCommand()
        {
            uuid = Guid.NewGuid().ToString();
        }
        [Key]
        public string UUID
        {
            get
            {
                return uuid;
            }
            set
            {
                uuid = value;
            }
        }
        
        public string Alias
        {
            get
            {
                return alias;
            }
            set
            {
                alias = value;
                RaisePropertyChanged("Alias");
            }
        }

        public string Command
        {
            get
            {
                return command;
            }
            set
            {
                command = value;
                RaisePropertyChanged("Command");
            }
        }
        [ForeignKey("ParentId")]
        public virtual ICollection<QuickCommandEnvConfig> QuickCommandEnvConfigs { get; set; }

        public string Path
        {
            get
            {
                return path;
            }
            set
            {
                path = value;
                try
                {
                    //FileInfo fInfo = new FileInfo(Path);
                    isDirectory = Directory.Exists(Path);// (fInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory;
                }
                catch (UnauthorizedAccessException e)
                {

                }
                catch (ArgumentException e)
                { }




                try
                {
                    if(!isDirectory)
                    {
                        System.Drawing.Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(path);
                        Img = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(icon.Handle, new Int32Rect(0, 0, icon.Width, icon.Height), BitmapSizeOptions.FromEmptyOptions());
                        
                    }
                }
                catch (Exception e)
                {

                }

                RaisePropertyChanged("Path");
            }
        }

        public string WorkDirectory
        {
            get
            {
                return workDirectory;
            }
            set
            {
                workDirectory = value;
                RaisePropertyChanged("WorkDirectory");
            }
        }

        [NotMapped]
        public ImageSource Img
        {
            get
            {
                return img;
            }
            set
            {
                img = value;
                RaisePropertyChanged("Img");
            }
        }

        [NotMapped]
        public System.Windows.Visibility ImgVisibility
        {
            get
            {
                return img == null ? System.Windows.Visibility.Hidden:System.Windows.Visibility.Visible;
            }
        }

        public override bool Equals(object obj)
        {
            var other = obj as QuickCommand;
            if(other != null)
            {
                return Alias == other.Alias;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Alias.GetHashCode();
        }
        [NotMapped]
        public System.Windows.Visibility DelVisible
        {
            get
            {
                if(Alias.Trim().Length > 0)
                {
                    return System.Windows.Visibility.Visible;
                }
                else
                {
                    return System.Windows.Visibility.Hidden;
                }
            }
        }

        [NotMapped]
        public bool DelEnabled
        {
            get
            {
                return Alias.Trim().Length > 0;
            }
        }

        [NotMapped]
        public System.Windows.Visibility EditVisible
        {
            get
            {
                if (!isDirectory)
                {
                    return System.Windows.Visibility.Visible;
                }
                else
                {
                    return System.Windows.Visibility.Hidden;
                }
            }
        }

        [NotMapped]
        public bool EditEnabled
        {
            get
            {
                return !isDirectory;
            }
        }
    }
}
