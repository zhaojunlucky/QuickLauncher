using QuickLauncher.Converter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Utility.Model;

namespace QuickLauncher.Model
{
    [Table("QUICK_COMMAND")]
    public class QuickCommand : AbstractNotifyPropertyChanged, IDataErrorInfo
    {
        private string uuid = "";
        private string alias = "";
        private string command = "";
        private string path = "";
        private int autoStart;
        private ImageSource img;
        private string workDirectory = "";
        private byte[] customIcon;
        private bool isReadOnly;
        private static readonly System.Windows.Media.Brush ErrorBgBrush = (System.Windows.Media.Brush)Application.Current.FindResource("MahApps.Brushes.ValidationSummary4");

        public QuickCommand(bool isNew)
        {
            if (isNew)
            {
                uuid = Guid.NewGuid().ToString();
            }
            IsNew = isNew;
        }

        public QuickCommand(QuickCommand quickCommand)
            : this(true)
        {
            alias = quickCommand.alias + "_Copy";
            command = quickCommand.command;
            path = quickCommand.path;
            autoStart = 0;
            workDirectory = quickCommand.workDirectory;
            customIcon = quickCommand.customIcon;
            img = quickCommand.Img;
        }

        public static QuickCommand Copy(QuickCommand quickCommand)
        {
            var cmd = new QuickCommand
            {
                alias = quickCommand.alias,
                command = quickCommand.command,
                path = quickCommand.path,
                autoStart = quickCommand.autoStart,
                uuid = quickCommand.uuid,
                workDirectory = quickCommand.workDirectory,
                customIcon = quickCommand.customIcon,
                img = quickCommand.Img
            };
            return cmd;
        }

        public QuickCommand()
        {
            IsNew = false;
        }

        [Key]
        [Column("UUID")]
        public string Uuid
        {
            get => uuid;
            set
            {
                uuid = value;
                RaisePropertyChanged("Uuid");
            }
        }

        public string Alias
        {
            get => alias;
            set
            {
                alias = value;
                RaisePropertyChanged("Alias");
            }
        }

        public string Command
        {
            get => command;
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
            get => path;
            set
            {
                path = value;
                RaisePropertyChanged("Path");
            }
        }

        [Column("AUTO_START")]
        public bool IsAutoStart
        {
            get => autoStart > 0;
            set
            {
                autoStart = value ? 1 : 0;
                RaisePropertyChanged("IsAutoStart");
            }
        }

        [NotMapped]
        public string ExpandedPath => Environment.ExpandEnvironmentVariables(Path);

        [NotMapped]
        public string ExpandedWorkDirectory => Environment.ExpandEnvironmentVariables(WorkDirectory);

        public string WorkDirectory
        {
            get => workDirectory;
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
                if (img != null)
                {
                    return img;
                }

                if (CustomIcon != null)
                {
                    img = ByteToImageSourceConverter.ConvertByteToImage(CustomIcon);
                }
                else
                {
                    LoadImgFromPath();
                }

                if (img != null)
                {
                    RaisePropertyChanged("Img");
                    RaisePropertyChanged("ImgVisibility");

                }
                return img;
            }
            set
            {
                img = value;
                customIcon = ByteToImageSourceConverter.ImageSourceToBytes(img);
                RaisePropertyChanged("Img");
                RaisePropertyChanged("ImgVisibility");
            }
        }

        public byte[] CustomIcon
        {
            get => customIcon;
            set
            {
                customIcon = value;
                img = ByteToImageSourceConverter.ConvertByteToImage(value);
                RaisePropertyChanged("CustomIcon");
                RaisePropertyChanged("Img");
                RaisePropertyChanged("ImgVisibility");
            }
        }

        [NotMapped]
        public Visibility ImgVisibility => img == null ? Visibility.Hidden : Visibility.Visible;

        [NotMapped]
        public bool IsNew { get; set; }

        [NotMapped]
        public bool IsReadOnly
        {
            get => isReadOnly;
            set
            {
                isReadOnly = value;
                RaisePropertyChanged("IsReadOnly");
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is QuickCommand other)
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
        public System.Windows.Media.Brush ErrorBackground
        {
            get
            {
                if (!File.Exists(ExpandedPath) || !Directory.Exists(ExpandedWorkDirectory))
                {
                    return ErrorBgBrush;
                }
                else
                {
                    return System.Windows.Media.Brushes.Transparent;
                }
            }
        }

        private bool CheckAlias(string aliasToCheck)
        {
            var dbContext = QuickCommandContext.Instance;
            var existingComm = from b in dbContext.QuickCommands
                               where b.Alias == aliasToCheck
                               select b;
            int cnt = 0;
            foreach (var _ in existingComm)
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
                {
                    Trace.TraceWarning(error);
                    return "Please check input with red border and correct";
                }
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
                        return "Application path is required.";
                    }
                    else if (!File.Exists(ExpandedPath))
                    {
                        return "Application path doesn't exist.";
                    }
                }
                else if (name == "WorkDirectory")
                {
                    if (WorkDirectory == null || WorkDirectory.Trim().Length == 0)
                    {
                        return "Application work directory is required.";
                    }
                    else if (!Directory.Exists(ExpandedWorkDirectory))
                    {
                        return "Application work directory doesn't exist.";
                    }
                }
                return null;
            }
        }

        public void PathChanged()
        {
            //if (string.IsNullOrEmpty(WorkDirectory) || !Directory.Exists(WorkDirectory))
            //{
            //    WorkDirectory = FileUtil.getDirectoryOfFile(Path);
            //}

            //if (string.IsNullOrEmpty(Alias))
            //{
            //    Alias = FileUtil.getFileNameNoExt(path);
            //}

            // if user didn't custom the icon, then load from path
            if (CustomIcon == null)
            {
                LoadImgFromPath();
            }
            RaisePropertyChanged("ErrorBackground");
        }

        private void LoadImgFromPath()
        {
            if (File.Exists(ExpandedPath))
            {
                Icon icon = Icon.ExtractAssociatedIcon(ExpandedPath);
                img = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(icon.Handle, new Int32Rect(0, 0, icon.Width, icon.Height), BitmapSizeOptions.FromEmptyOptions());
                RaisePropertyChanged("Img");
                RaisePropertyChanged("ImgVisibility");
            }
        }
    }
}
