using System.ComponentModel.DataAnnotations.Schema;
using Utility.Model;

namespace QuickLauncher.Model
{
    [Table("SETTING")]
    public class SettingItem : AbstractNotifyPropertyChanged
    {
        private string key = "";
        private string value = "";

        [System.ComponentModel.DataAnnotations.Key, Column("KEY")]
        public string Key
        {
            get => key;
            set
            {
                key = value;
                RaisePropertyChanged("key");
            }
        }
        [Column("VALUE")]
        public string Value
        {
            get => value;
            set
            {
                this.value = value;
                RaisePropertyChanged("value");
            }
        }
    }
}
