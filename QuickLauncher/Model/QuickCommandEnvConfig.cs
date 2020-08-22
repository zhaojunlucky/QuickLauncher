using QuickLauncher.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Utility.Model;

namespace QuickLauncher.Model
{
    [Table("QUICK_COMMAND_ENV_CONFIG")]
    public class QuickCommandEnvConfig : AbstractNotifyPropertyChanged, IDataErrorInfo
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

        [NotMapped]
        public string ExpandedEnvValue
        {
            get
            {
                return Environment.ExpandEnvironmentVariables(EnvValue);
            }
        }

        [NotMapped]
        public ObservableCollection<QuickCommandEnvConfig> BindingEnvs
        {
            get;
            set;
        }

        public string Error
        {
            get
            {
                string error = this["EnvKey"];
                if (!string.IsNullOrEmpty(error))
                    return "Please check input with red border and correct";
                return null;
            }
        }

        public string this[string name]
        {
            get
            {
                if (name == "EnvKey")
                {
                    if (EnvKey.Trim() == "")
                    {
                        return "Key is required, and can't be all spaces.";
                    }
                    else if (checkDup(EnvKey))
                    {
                        return string.Format("Key {0} is duplicate!!!", EnvKey);
                    }
                }
                return null;
            }
        }

        private bool checkDup(string key)
        {
            if (BindingEnvs == null)
            {
                return false;
            }
            else
            {
                int cnt = 0;
                foreach (var o in BindingEnvs)
                {
                    if (o.EnvKey.Trim() == key.Trim())
                    {
                        ++cnt;
                    }
                }
                return cnt >= 2;
            }

        }

        public override bool Equals(object obj)
        {
            var other = obj as QuickCommandEnvConfig;
            if (other != null)
            {
                return EnvKey == other.EnvKey;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return EnvKey.GetHashCode();
        }
    }
}
