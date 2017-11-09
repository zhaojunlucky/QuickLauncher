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
    [Table("QUICK_COMMAND_ENV_CONFIG")]
    public class QuickCommandEnvConfig : AbstractNotifyPropertyChanged
    {
        private long id = 0;
        private string parentId = "";
        private string envKey = "";
        private string envValue = "";

        [Key]
        [DatabaseGeneratedAttribute(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)]
        public long Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }
        [Column("PARENT_ID")]
        public string ParentId
        {
            get
            {
                return parentId;
            }
            set
            {
                parentId = value;
                RaisePropertyChanged("parentId");
            }
        }
        [Column("ENV_KEY")]
        public string EnvKey
        {
            get
            {
                return envKey;
            }
            set
            {
                envKey = value;
                RaisePropertyChanged("envKey");
            }
        }
        [Column("ENV_VALUE")]
        public string EnvValue
        {
            get
            {
                return envValue;
            }
            set
            {
                envValue = value;
                RaisePropertyChanged("envValue");
            }
        }
    }
}
