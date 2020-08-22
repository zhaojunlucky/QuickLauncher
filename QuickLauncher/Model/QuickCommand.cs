using QuickLauncher.Converter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Utility.Model;
using System.Linq;

namespace QuickLauncher.Model
{
    [Table("QUICK_COMMAND")]
    public class QuickCommand : AbstractNotifyPropertyChanged, IDataErrorInfo
    {
        private string uuid = "";
        private string alias = "";
        private string command = "";
        private string path = "";
        private ImageSource img = null;
        private string workDirectory = "";
        private byte[] customeIcon = null;

        public QuickCommand(bool isNew)
        {
            if (isNew)
            {
                uuid = Guid.NewGuid().ToString();
            }
            IsNew = isNew;
        }

        public QuickCommand()
        {
            IsNew = false;
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
                RaisePropertyChanged("Path");
            }
        }

        [NotMapped]
        public string ExpandedPath
        {
            get
            {
                return Environment.ExpandEnvironmentVariables(Path);
            }
        }

        [NotMapped]
        public string ExpandedWorkDirectory
        {
            get
            {
                return Environment.ExpandEnvironmentVariables(WorkDirectory);
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
                if (img == null && File.Exists(ExpandedPath))
                {
                    System.Drawing.Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(ExpandedPath);
                    Img = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(icon.Handle, new Int32Rect(0, 0, icon.Width, icon.Height), BitmapSizeOptions.FromEmptyOptions());
                }
                return img;
            }
            set
            {
                img = value;
                customeIcon = ByteToImageSourceConverter.ImageSourceToBytes(img);
                RaisePropertyChanged("Img");
                RaisePropertyChanged("ImgVisibility");
            }
        }

        public byte[] CustomIcon
        { 
            get
            {
                return customeIcon;
            }
            set
            {
                customeIcon = value;
                img = ByteToImageSourceConverter.ConvertByteToImage(customeIcon);
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

        [NotMapped]
        public bool IsNew { get; set; }

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
        public bool DelEnabled
        {
            get
            {
                return Alias.Trim().Length > 0;
            }
        }

        private bool CheckAlias(string alias)
        {
            var dbContext = QuickCommandContext.Instance;
            var existingComm = from b in dbContext.QuickCommands
                               where b.Alias == Alias
                               select b;
            int cnt = 0;
            foreach (var comm in existingComm)
            {
                ++cnt;
            }
            return IsNew ? cnt > 0 : cnt > 1;
        }

        public string Error
        {
            get
            {
                string error = this["Alias"] + this["Path"] + this["WorkDirectory"];
                if (!string.IsNullOrEmpty(error))
                    return "Please check input with red border and correct";
                return null;
            }
        }

        public string this[string name]
        {
            get
            {
                if (name == "Alias")
                {
                    if (Alias == null || Alias.Trim().Length == 0)
                    {
                        return "Alias is required, and can't be all spaces.";
                    }
                    else if (CheckAlias(Alias))
                    {
                        return "Alias already exists.";
                    }
                }
                else if (name == "Path")
                {
                    if (Path == null || Path.Trim().Length == 0)
                    {
                        return "Aplication path is required.";
                    }
                    else if (!File.Exists(ExpandedPath))
                    {
                        return "Aplication path doesn't exist.";
                    }
                }
                else if (name == "WorkDirectory")
                {
                    if (WorkDirectory == null || WorkDirectory.Trim().Length == 0)
                    {
                        return "Aplication work directory is required.";
                    }
                    else if (!Directory.Exists(ExpandedWorkDirectory))
                    {
                        return "Aplication  work directory doesn't exist.";
                    }
                }
                return null;
            }
        }

        public void PathChanged()
        {
            if (WorkDirectory == null || WorkDirectory.Trim().Length == 0)
            {
                WorkDirectory = FileUtil.getParentDir(path);
            }

            if (Alias == null || Alias.Trim().Length == 0)
            {
                Alias = FileUtil.getFileNameNoExt(path);
            }
            // trigger img changed
            var img = Img;
        }
    }
}
