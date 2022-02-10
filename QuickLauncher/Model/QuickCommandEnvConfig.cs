using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using Utility.Model;

namespace QuickLauncher.Model
{
    [Table("QUICK_COMMAND_ENV_CONFIG")]
    public class QuickCommandEnvConfig : AbstractNotifyPropertyChanged, IDataErrorInfo
    {
        private string parentId = "";
        private string envKey = "";
        private string envValue = "";

        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; } = 0;

        [Column("PARENT_ID")]
        public string ParentId
        {
            get => parentId;
            set
            {
                parentId = value;
                RaisePropertyChanged("parentId");
            }
        }
        [Column("ENV_KEY")]
        public string EnvKey
        {
            get => envKey;
            set
            {
                envKey = value;
                RaisePropertyChanged("envKey");
            }
        }
        [Column("ENV_VALUE")]
        public string EnvValue
        {
            get => envValue;
            set
            {
                envValue = value;
                RaisePropertyChanged("envValue");
            }
        }

        [NotMapped]
        public string ExpandedEnvValue => Environment.ExpandEnvironmentVariables(EnvValue);

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
                if (name == "EnvKey")
                {
                    if (EnvKey.Trim() == "")
                    {
                        return "Key is required, and can't be all spaces.";
                    }
                    else if (CheckDup(EnvKey))
                    {
                        return $"Key {EnvKey} is duplicate!!!";
                    }
                }
                return null;
            }
        }

        private bool CheckDup(string key)
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
            if (obj is QuickCommandEnvConfig other)
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
